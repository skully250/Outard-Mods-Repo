using UnityEngine;

namespace MPLimitRemover
{
    public class MPScriptLoad : MonoBehaviour
    {
        public static PlayerLimitRemover plm;
        public void Initialise()
        {
            Patch();
        }

        public void Patch()
        {
            //On.StoreManager.OnGameLobbyJoinRequested += new On.StoreManager.hook_OnGameLobbyJoinRequested(GameLobbyJoinRequest);
            On.ConnectPhotonMaster.CreateOrJoin += new On.ConnectPhotonMaster.hook_CreateOrJoin(CreateJoinRoom);
            On.ConnectPhotonMaster.CreateRoom_1 += new On.ConnectPhotonMaster.hook_CreateRoom_1(CreateStoreRoom);
            //Put in patched methods here
        }

        /*public void GameLobbyJoinRequest(On.StoreManager.orig_OnGameLobbyJoinRequested original, StoreManager self, string _roomName)
        {
            string text = string.Empty;
            if (!DemoManager.DemoIsActive)
            {
                if (Global.GamePaused)
                {
                    PauseMenu.Pause(false);
                }
                if (NetworkLevelLoader.Instance.IsOverallLoadingDone && NetworkLevelLoader.Instance.ContinueAfterLoading && SplitScreenManager.Instance.LocalPlayerCount == 1)
                {
                    Character firstLocalCharacter = CharacterManager.Instance.GetFirstLocalCharacter();
                    if (firstLocalCharacter == null || firstLocalCharacter.IsDead)
                    {
                        text = "MessageBox_Network_CannotJoinAtThisMoment";
                    }
                    else if (true)
                    {
                        self.JoinRequestFromLobbyID(_roomName);
                    }
                    else
                    {
                        self.SetPendingRequestConnectionToRoom(_roomName);
                    }
                }
                else if (Global.Lobby.PlayersInLobby.Count == 0)
                {
                    text = "MessageBox_Network_JoinNoCharacter";
                }
                else if (Global.Lobby.LocalPlayerCount == 2 || SplitScreenManager.Instance.LocalPlayerCount > 1)
                {
                    text = "MessageBox_Network_JoinSplitscreen";
                }
                else
                {
                    text = "Connection_Error_JoinRoomFailed";
                }
            }
            else
            {
                text = "MessageBox_Network_JoinTutorial";
            }
            if (!string.IsNullOrEmpty(text))
            {
                string loc = LocalizationManager.Instance.GetLoc(text);
                CharacterUI characterUI = SplitScreenManager.Instance.GetCharacterUI(0);
                if (characterUI)
                {
                    characterUI.MessagePanel.Show(loc, -1f, null);
                    if (characterUI.PauseMenu && characterUI.PauseMenu.IsDisplayed)
                    {
                        characterUI.PauseMenu.Hide();
                    }
                }
            }
        }*/

        public void CreateJoinRoom(On.ConnectPhotonMaster.orig_CreateOrJoin original, ConnectPhotonMaster self, string _roomName)
        {
            PhotonNetwork.JoinOrCreateRoom(_roomName, new RoomOptions
            {
                isVisible = true,
                //Configurable
                maxPlayers = PlayerLimitRemover.PlayerLimit
            }, TypedLobby.Default);
        }

        public void CreateStoreRoom(On.ConnectPhotonMaster.orig_CreateRoom_1 original, ConnectPhotonMaster self, string _roomName, int _storeID, string _lobbyID)
        {
            RoomOptions roomOptions = new RoomOptions();
            if (_storeID != -1 && !string.IsNullOrEmpty(_lobbyID))
            {
                roomOptions.customRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                {
                    "Store",
                    _storeID
                },
                {
                    "LobbyID",
                    _lobbyID
                }
            };
            }
            roomOptions.isVisible = true;
            roomOptions.maxPlayers = PlayerLimitRemover.PlayerLimit;
            PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
        }
    }
}
