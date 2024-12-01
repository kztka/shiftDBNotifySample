using System.Collections.Generic;
using UnityEngine;
using Microsoft.Data.Sqlite;
using SqlKata.Execution;
using SqlKata.Compilers;
using System;
using System.Reflection;

/// <summary>
/// SQLite3のDBアクセス制御クラス
/// </summary>
public static class DbAccessController
{
    public static QueryFactory createDbConnection()
    {
        if( null != StaticParameters.queryFactory ){
            Logger.DebugLog("DB接続存在済みの為return");
            return StaticParameters.queryFactory;
        }

        DeleteDB();
        CloneDB();

        SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder
        {
            DataSource = System.IO.Path.Combine(Application.persistentDataPath, StaticParameters.s_dbName)
        };

        SQLitePCL.Batteries_V2.Init();

        Logger.DebugLog("接続確認");

        StaticParameters.dbConnection = new SqliteConnection(builder.ConnectionString);
        StaticParameters.dbConnection.Open();  // ここでOpenしなくてもqueryFactoryでクエリ時に勝手にOpenになるがDB更新のフックが登録できないのでOpenする

        // SQLiteのバージョン出力
        Logger.DebugLog("バージョン情報：" + StaticParameters.dbConnection.ServerVersion);

        SqliteCompiler compiler = new SqliteCompiler();
        StaticParameters.queryFactory = new QueryFactory(StaticParameters.dbConnection, compiler);

        // DB更新通知受信コールバック関数登録
        Logger.DebugLog("DB更新通知受信コールバック関数登録");
        SQLitePCL.raw.sqlite3_update_hook(StaticParameters.dbConnection.Handle, pushNotifyUpdateDbCB, null);

        return StaticParameters.queryFactory;
    }

    public static void closeDbConnection()
    {
        Logger.DebugLog("DB connection close実施");
        StaticParameters.queryFactory.Connection.Close();
    }

    public static QueryFactory getDbQueryFactory()
    {
        return StaticParameters.queryFactory;
    }


    // DB更新通知push受信コールバック関数ディクショナリー初期化
    // 本メソッドはDBへの初期テーブルインポート完了後に実施する事
    public static void initReceiveNotifyUpdateDbCBDic()
    {
        Logger.DebugLog("initReceiveNotifyUpdateDbCBDic START");
        foreach (sqlite_master table in StaticParameters.queryFactory.Query(nameof(sqlite_master)).
                                                                    WhereColumns(nameof(sqlite_master) + "." + nameof(sqlite_master.type), "=", "table").
                                                                    Get<sqlite_master>())
        {
            Logger.DebugLog("sqlite_master-> " + table.ToStringReflection());
            StaticParameters.receiveNotifyUpdateDbCBDic[table.name] = new List<Func<object, int, string, string, long, int>>();
        }
        Logger.DebugLog("initReceiveNotifyUpdateDbCBDic END");
    }

    // DB更新通知push受信コールバック関数ディクショナリー追加
    public static void addReceiveNotifyUpdateDbCBDic( string targetTable, Func<object, int, string, string, long, int> addFunc){
        Logger.DebugLog("addReceiveNotifyUpdateDbCBDic START targetTable:" + targetTable + " addFunc:" + addFunc.Method.Name);
        StaticParameters.receiveNotifyUpdateDbCBDic[targetTable].Add(addFunc);
        Logger.DebugLog("addReceiveNotifyUpdateDbCBDic END DicCount:" + StaticParameters.receiveNotifyUpdateDbCBDic.Count + " TableCount:" + StaticParameters.receiveNotifyUpdateDbCBDic[targetTable].Count);
    }

    // DB更新通知push受信コールバック関数ディクショナリー削除
    public static void removeReceiveNotifyUpdateDbCBDic( string targetTable, Func<object, int, string, string, long, int> delFunc){
        Logger.DebugLog("removeReceiveNotifyUpdateDbCBDic START targetTable:" + targetTable + " delFunc:" + delFunc.Method.Name);
        StaticParameters.receiveNotifyUpdateDbCBDic[targetTable].Remove(delFunc);
        Logger.DebugLog("removeReceiveNotifyUpdateDbCBDic END DicCount:" + StaticParameters.receiveNotifyUpdateDbCBDic.Count + " TableCount:" + StaticParameters.receiveNotifyUpdateDbCBDic[targetTable].Count);
    }

    // DB更新通知受信コールバック関数(各通知先へpush)
    // 引数はSQLitePCL.delegate_updateを参照
    static void pushNotifyUpdateDbCB(object userData, int queryType, SQLitePCL.utf8z updateDB, SQLitePCL.utf8z updateTable, long updateRowID){
        string updateDBstr = updateDB.utf8_to_string();
        string updateTablestr = updateTable.utf8_to_string();

        Logger.DebugLog("pushNotifyUpdateDbCB START queryType:" + queryType + " updateDB:" + updateDBstr + " updateTable:" + updateTablestr + " updateRowID:" + updateRowID);
        Logger.DebugLog("StaticParameters.receiveNotifyUpdateDbCBList Count:" + StaticParameters.receiveNotifyUpdateDbCBDic.Count);

        // Push通知先関数実行
        if( StaticParameters.receiveNotifyUpdateDbCBDic.ContainsKey(updateTablestr)){
            Logger.DebugLog("receiveNotifyUpdateDbCBList contain FuncList of " + updateTablestr);
            List<Func<object, int, string, string, long, int>> receiveNotifyUpdateDbCBList = StaticParameters.receiveNotifyUpdateDbCBDic[updateTablestr];
            if( receiveNotifyUpdateDbCBList is not null ){
                Logger.DebugLog("receiveNotifyUpdateDbCBList is not null count:" + receiveNotifyUpdateDbCBList.Count);
                foreach( Func<object, int, string, string, long, int> receiveNotifyUpdateDbCB in receiveNotifyUpdateDbCBList ){
                    Logger.DebugLog("execute start " + receiveNotifyUpdateDbCB.GetMethodInfo().Name);
                    int result = receiveNotifyUpdateDbCB(userData, queryType, updateDBstr, updateTablestr, updateRowID);
                    Logger.DebugLog("execute end " + receiveNotifyUpdateDbCB.GetMethodInfo().Name + " result:" + result);
                }
            } else {
                Logger.DebugLog("receiveNotifyUpdateDbCBList is null, skip");
            }
        } else {
            Logger.DebugLog("receiveNotifyUpdateDbCBList don't contain FuncList of " + updateTablestr + ", skip");
        }

        Logger.DebugLog("pushNotifyUpdateDbCB END");
    }

    static void CloneDB()
    {
        string targetPath = System.IO.Path.Combine(Application.persistentDataPath, StaticParameters.s_dbName);
        if (System.IO.File.Exists(targetPath)){
            Logger.DebugLog(targetPath + "にDB有り");
            return;
        }
        string sourcePath = System.IO.Path.Combine(Application.streamingAssetsPath, StaticParameters.s_dbName);
        if (!System.IO.File.Exists(sourcePath)){
            Logger.DebugLog(sourcePath + "にDB無しのため新規作成");
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder
            {
                DataSource = sourcePath
            };
            using (SqliteConnection connection = new SqliteConnection(builder.ConnectionString))
            {
                connection.Open();
                connection.Close();
            }
        }
        System.IO.File.Copy(sourcePath, targetPath);
    }

    static void DeleteDB()
    {
        string targetPath = System.IO.Path.Combine(Application.persistentDataPath, StaticParameters.s_dbName);
        if (System.IO.File.Exists(targetPath)){
            Logger.DebugLog(targetPath + "にDB有りの為削除");
            System.IO.File.Delete(targetPath);
        }
        string sourcePath = System.IO.Path.Combine(Application.streamingAssetsPath, StaticParameters.s_dbName);
        if (System.IO.File.Exists(sourcePath)){
            Logger.DebugLog(sourcePath + "にDB有りの為削除");
            System.IO.File.Delete(sourcePath);
        }
    }

}
