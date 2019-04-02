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
            On.OtherPlayersCompass.Update += new On.OtherPlayersCompass.hook_Update(CompassUpdate);
            //Put in patched methods here
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
    }
}
