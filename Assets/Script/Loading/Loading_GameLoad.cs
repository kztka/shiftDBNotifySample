using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SqlKata.Execution;

public class Loading_GameLoad : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text loadingText;

    // Start is called before the first frame update
    void Start()
    {
        slider.value = 0;
        // DB接続
        loadingText.text = "create DB connection...";
        QueryFactory factory = DbAccessController.createDbConnection();
        Logger.DebugLog("DB接続完了");

        slider.value = 0.1f;
        // 設定ファイルの読み込み
        loadingText.text = "SettingFile Loading...";
        SettingFileController.loadSettingFile();
        Logger.DebugLog("設定ファイルの読み込み完了");

        slider.value = 0.2f;
        // ゲームファイルのDB書き込み
        loadingText.text = "GameFile Loading...";
        DBDataExchanger.importInitialSqlToDB(factory);
        // DB更新通知push受信コールバック関数リスト初期化
        DbAccessController.initReceiveNotifyUpdateDbCBDic();
        Logger.DebugLog("ゲームファイルのDB書き込み完了");

        //100% 次画面遷移
        slider.value = 1;
        StaticParameters.playingData.gamename = "Game1";  // ゲーム名仮値
        StaticParameters.playingData.currentTurn = 1;  // 初期ターン数を設定
        StaticParameters.playingData.playerNationName = "EarthEmpire";  // 国名仮値
        Logger.DebugLog("StaticParameters.playingData:" + StaticParameters.playingData.ToStringReflection());
        SceneManager.LoadScene("StrategyMap");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
