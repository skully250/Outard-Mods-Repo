using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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
            //Put in patched methods here
            //On.StoreManager.OnGameLobbyJoinRequested += new On.StoreManager.hook_OnGameLobbyJoinRequested(GameLobbyJoinRequest);
            On.ConnectPhotonMaster.CreateOrJoin += new On.ConnectPhotonMaster.hook_CreateOrJoin(CreateJoinRoom);
            On.ConnectPhotonMaster.CreateRoom_1 += new On.ConnectPhotonMaster.hook_CreateRoom_1(CreateStoreRoom);
            On.OtherPlayersCompass.Update += new On.OtherPlayersCompass.hook_Update(CompassUpdate);
            On.PauseMenu.Show += new On.PauseMenu.hook_Show(ShowPatch);
            On.PauseMenu.Update += new On.PauseMenu.hook_Update(UpdatePatch);
            
        }

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

        private float timer = 5f;
        public void CompassUpdate(On.OtherPlayersCompass.orig_Update original, OtherPlayersCompass self)
        {
            original.Invoke(self);
            timer -= Time.deltaTime;
            if (timer <= 0f)
                Debug.Log(Global.Lobby.PlayersInLobby.Count);
        }

        public static void ShowPatch(On.PauseMenu.orig_Show original, PauseMenu instance)
        {
            original(instance);
            Button onlineButton = typeof(PauseMenu).GetField("m_btnToggleNetwork", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance) as Button;

            //Due to spawning bugs, only allow disconnect if you are the master, or if you are a client with no splitscreen, force splitscreen to quit before disconnect
            if (PhotonNetwork.isMasterClient || SplitScreenManager.Instance.LocalPlayerCount == 1)
            {
                onlineButton.interactable = true;
            }

            SetSplitButtonInteractable(instance);

            //If this is used with a second splitscreen player both players load in missing inventory. Very BAD. Disabled for now.
            //Button findMatchButton = typeof(PauseMenu).GetField("m_btnFindMatch", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance) as Button;
            //findMatchButton.interactable = PhotonNetwork.offlineMode;
        }

        //for some reason the update function also forces the split button interactable, so we have to override it here too
        public static void UpdatePatch(On.PauseMenu.orig_Update orignal, PauseMenu instance)
        {
            orignal(instance);
            SetSplitButtonInteractable(instance);
        }

        public static void SetSplitButtonInteractable(PauseMenu instance)
        {
            //Debug.Log("isMasterClient: " + PhotonNetwork.isMasterClient);
            Button splitButton = typeof(PauseMenu).GetField("m_btnSplit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance) as Button;
            if (!PhotonNetwork.isMasterClient || !PhotonNetwork.isNonMasterClientInRoom)
            {
                splitButton.interactable = true;
            }
        }
    }
}
