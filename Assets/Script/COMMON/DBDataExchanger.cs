using System.Collections.Generic;
using SqlKata.Execution;
using System.IO;

public static class DBDataExchanger
{
    public static void importInitialSqlToDB(QueryFactory factory)
    {
        Logger.DebugLog("importInitialSqlToDB start");
        // CREATE TABLEファイルリスト取得
        List<string> createSqlFiles = FileSearch.searchByExtentionSubDir(StaticParameters.createSqlDir, ".sql" );
        // CREATE TABLE実行
        foreach( string createSqlFile in createSqlFiles){
            Logger.DebugLog("createSqlFile:" + createSqlFile);
            string createSqlText = "";
            using(var reader = new StreamReader(createSqlFile)){
                createSqlText = reader.ReadToEnd();
            }
            
            factory.Statement(createSqlText);
            Logger.DebugLog("execute: " + createSqlText);
        }

        // INSERTファイルリスト取得
        List<string> insertSqlFiles = FileSearch.searchByExtentionSubDir(StaticParameters.insertSqlDir, ".sql" );

        // INSERT実行
        foreach( string insertSqlFile in insertSqlFiles){
            Logger.DebugLog("insertSqlFile:" + insertSqlFile);
            string insertSqlText = "";
            using(var reader = new StreamReader(insertSqlFile)){
                insertSqlText = reader.ReadToEnd();
            }
            
            factory.Statement(insertSqlText);
            Logger.DebugLog("execute: " + insertSqlText);
        }

        Logger.DebugLog("importInitialSqlToDB end");
    }
}
