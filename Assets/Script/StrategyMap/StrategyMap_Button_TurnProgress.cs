using UnityEngine;
using TMPro;
using SqlKata.Execution;

public class StrategyMap_Button_TurnProgress : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentTurnText;

    // Start is called before the first frame update
    void Start()
    {
        Logger.DebugLog("StrategyMap_Button_TurnProgress Start start");
        // 格納されている値でテキストのターン数を更新
        currentTurnText.text = "Turn " + StaticParameters.playingData.currentTurn;
        Logger.DebugLog("StrategyMap_Button_TurnProgress Start end currentTurnText.text: " + currentTurnText.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // clickButton is called when player clicked return button
    public void clickButton()
    {
        Logger.DebugLog("StrategyMap_Button_TurnProgress onclick start");
        // 現在のターン数を更新
        StaticParameters.playingData.currentTurn++;
        // テキストのターン数を更新
        currentTurnText.text = "Turn " + StaticParameters.playingData.currentTurn;
        // TODO: 仮でリソースを固定値で更新
        QueryFactory factory = DbAccessController.getDbQueryFactory();
        foreach (ResourcePerNationsByGame table in factory.Query(nameof(ResourcePerNationsByGame))
                                                          .WhereColumns(nameof(ResourcePerNationsByGame) + "." + nameof(ResourcePerNationsByGame.gamename), "=", StaticParameters.playingData.gamename)
                                                          .WhereColumns(nameof(ResourcePerNationsByGame) + "." + nameof(ResourcePerNationsByGame.nationName), "=", StaticParameters.playingData.playerNationName)
                                                          .Get<ResourcePerNationsByGame>())
        {
            Logger.DebugLog("ResourcePerNationsByGame-> " + table.ToStringReflection());
            table.resourceAmount += 100;
            factory.Query(nameof(ResourcePerNationsByGame))
                   .WhereColumns(nameof(ResourcePerNationsByGame) + "." + nameof(ResourcePerNationsByGame.gamename), "=", StaticParameters.playingData.gamename)
                   .WhereColumns(nameof(ResourcePerNationsByGame) + "." + nameof(ResourcePerNationsByGame.nationName), "=", StaticParameters.playingData.playerNationName)
                   .WhereColumns(nameof(ResourcePerNationsByGame) + "." + nameof(ResourcePerNationsByGame.resourceName), "=", table.resourceName)
                   .Update(table);
            Logger.DebugLog("Updated ResourcePerNationsByGame-> " + table.ToStringReflection());
        }
        Logger.DebugLog("StrategyMap_Button_TurnProgress onclick end currentTurnText.text: " + currentTurnText.text);
    }
}
