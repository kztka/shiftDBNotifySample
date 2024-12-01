using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SqlKata.Execution;
using Microsoft.Data.Sqlite;

/// <summary>
/// static変数管理クラス
/// </summary>
public static class StaticParameters{

    public static readonly string createSqlDir = ".\\Assets\\Data\\CREATE_TABLE"; // CREATE_TABLE sqlファイルディレクトリ名
    public static readonly string insertSqlDir = ".\\Assets\\Data\\INSERT"; // INSERT sqlファイルディレクトリ名
    public static readonly string s_dbName = "mainDB.sqlite3";  // DB名
    public static QueryFactory queryFactory = null;  // SqlKataのQuery用オブジェクト
    public static SqliteConnection dbConnection = null;  // DBコネクション用オブジェクト
    public static PlayingData playingData = new PlayingData();  // プレイ中ゲーム情報(DBではない方が良いセーブ対象データはここに集約)
    public static readonly int pixelsPerUnit = 100;  // ゲーム全体のpixelsPerUnit値
    public static readonly float minCamOrthographicSize = 3; // 最大ズームインカメラサイズ
    public static float maxCamOrthographicSize = 25; // 最大ズームアウトカメラサイズ
    public static readonly string settingFileName = "settings.json";  // 全体設定ファイル名
    public static JObject settingParameters = null;  // 全体設定ファイルパラメータ格納先
    /*  DB更新通知push受信コールバック関数ディクショナリー
        -> 通知受信が必要になったタイミングで追加し、不要になったタイミングで削除する事。
        Dictionary<
          string -> 更新対象テーブル名。該当テーブル更新時のみValueの関数リストが実行される。
          List<Func> -> push通知受信コールバック関数リスト
            引数1：ユーザデータ。基本はnullなので参照不要
            引数2：DB更新の原因操作。SQLITE_INSERT、SQLITE_DELETE、またはSQLITE_UPDATEのいずれか
            引数3：更新対象DB名
            引数4：更新対象テーブル名
            引数5：更新対象rowid
            戻値：実行結果　※実行元は特にこの結果を意識しない
        >
    */
    public static Dictionary<string, List<Func<object, int, string, string, long, int>>> receiveNotifyUpdateDbCBDic = new Dictionary<string, List<Func<object, int, string, string, long, int>>>();
}
