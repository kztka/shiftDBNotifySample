using SqlKata.Execution;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StrategyMap_ResourcePanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Logger.DebugLog("StrategyMap_ResourcePanel OnStart START");
        SceneManager.sceneUnloaded += SceneUnloadedResourcePanel;  // シーン破棄時の実行関数追加
        // DB更新通知push受信コールバック関数を登録
        DbAccessController.addReceiveNotifyUpdateDbCBDic(nameof(ResourcePerNationsByGame), receiveResourceUpdateDbCB_StrategyMap);

        // 初回のリソースパネル更新を実施
        updateResourcePanel();
        
        Logger.DebugLog("StrategyMap_ResourcePanel OnStart END");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // SceneUnloaded is called after this scene is unloaded.
    void SceneUnloadedResourcePanel(Scene thisScene){
        Logger.DebugLog("StrategyMap_ResourcePanel OnSceneUnloaded START");
        SceneManager.sceneUnloaded -= SceneUnloadedResourcePanel;
        // DB更新通知push受信コールバック関数を削除
        DbAccessController.removeReceiveNotifyUpdateDbCBDic(nameof(ResourcePerNationsByGame), receiveResourceUpdateDbCB_StrategyMap);
        Logger.DebugLog("StrategyMap_ResourcePanel OnSceneUnloaded END");
    }

    // ResourcePerNationsByGameの変更通知を受け取った際にリソースパネルの表示数を更新する関数
    int receiveResourceUpdateDbCB_StrategyMap(object userData, int queryType, string updateDBstr, string updateTablestr, long updateRowID){
        Logger.DebugLog("receiveResourceUpdateDbCB START queryType:" + queryType + " updateDBstr:" + updateDBstr + " updateTablestr:" + updateTablestr + " updateRowID:" + updateRowID);
        int result = 0;

        updateResourcePanel();

        Logger.DebugLog("receiveResourceUpdateDbCB END");
        return result;
    }

    // リソースパネルの値を更新する
    void updateResourcePanel(){
        Logger.DebugLog("updateResourcePanel START");
        Logger.DebugLog("StaticParameters.playingData-> " + StaticParameters.playingData.ToStringReflection());

        QueryFactory factory = DbAccessController.getDbQueryFactory();
        foreach (ResourcePerNationsByGame table in factory.Query(nameof(ResourcePerNationsByGame))
                                                          .WhereColumns(nameof(ResourcePerNationsByGame) + "." + nameof(ResourcePerNationsByGame.gamename), "=", StaticParameters.playingData.gamename)
                                                          .WhereColumns(nameof(ResourcePerNationsByGame) + "." + nameof(ResourcePerNationsByGame.nationName), "=", StaticParameters.playingData.playerNationName)
                                                          .Get<ResourcePerNationsByGame>())
        {
            Logger.DebugLog("ResourcePerNationsByGame-> " + table.ToStringReflection());
            updateResourcePanelPerResource(table);
        }

        Logger.DebugLog("updateResourcePanel END");
    }

    // 個々のリソースの値を更新する
    void updateResourcePanelPerResource(ResourcePerNationsByGame dbData){
        Logger.DebugLog("updateResourcePanelPerResource START ResourcePerNationsByGame-> " + dbData.ToStringReflection());
        GameObject content = transform.Find( dbData.resourceName + "_AmountPanel/" + dbData.resourceName + "_AmountText").gameObject;
        TextMeshProUGUI amountText = content.GetComponent<TextMeshProUGUI>();
        Logger.DebugLog("beforeAmount ResourceName-> " + dbData.resourceName + " ResourceAmount-> " + amountText.text);
        amountText.text = dbData.resourceAmount.ToString();
        Logger.DebugLog("afterAmount ResourceName-> " + dbData.resourceName + " ResourceAmount-> " + amountText.text);

        Logger.DebugLog("updateResourcePanelPerResource END");
    }
}
