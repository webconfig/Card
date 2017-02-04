using Comm.Network;
using google.protobuf;
using Comm.Util;
using System;
using Orleans;
using CardGrainInterfaces;
using System.Threading.Tasks;
using System.Linq;

namespace GateServer
{
    public partial class ClientHandler : PacketHandlerManager<ClientData>
    {
        /// <summary>
        /// 客户端登陆
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(Op.Client.Login)]
        public void Login(BaseClient<ClientData> client, byte[] datas)
        {
            ClientLogin model_login;
            NetHelp.RecvData<ClientLogin>(datas, out model_login);

            ClientResult result = new ClientResult();
            result.Result = true;
            client.Send<ClientResult>(Op.Client.Login, result);
            client.t = new ClientData();
            client.t.playerId= Guid.NewGuid();
            Log.Debug("用户名：{0},密码：{1},playerId:{2}", model_login.UserName, model_login.Password, client.t.playerId.ToString());

        }

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(Op.Client.CreateRoom)]
        public async void CreateRoom(BaseClient<ClientData> client, byte[] datas)
        {
            CreateGameRequest model_create_room;
            NetHelp.RecvData<CreateGameRequest>(datas, out model_create_room);
            Log.Debug("用户名：{0},创建房间", model_create_room.UserName);


            var player = GrainClient.GrainFactory.GetGrain<IPlayerGrain>(client.t.playerId);
            var game_id = await player.CreateGame();

            CreateGameResult result = new CreateGameResult();
            result.RoomId = game_id.ToString();
            client.Send<CreateGameResult>(Op.Client.CreateRoom, result);
        }

        /// <summary>
        /// 查询所有房间
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(Op.Client.QueryRoom)]
        public async void QueryRoom(BaseClient<ClientData> client, byte[] datas)
        {
            QueryRoomRequest model_query_room;
            NetHelp.RecvData<QueryRoomRequest>(datas, out model_query_room);
            Log.Debug("用户名：{0},查询房间", model_query_room.RoomId);

            var grain = GrainClient.GrainFactory.GetGrain<IPairingGrain>(0);
            PairingSummary[] kkk=await grain.GetGames();

            QueryRoomResult result = new QueryRoomResult();
            for (int i = 0; i < kkk.Length; i++)
            {
                QueryRoomResult.QueryRoomResultItem item = new QueryRoomResult.QueryRoomResultItem();
                item.RoomId = kkk[i].GameId.ToByteArray();
                item.RoomName = kkk[i].Name;
                result.result.Add(item);
            }
            client.Send<QueryRoomResult>(Op.Client.QueryRoom, result);
        }


        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="client"></param>
        /// <param name="datas"></param>
        [PacketHandler(Op.Client.JoinRoom)]
        public async void JoinRoom(BaseClient<ClientData> client, byte[] datas)
        {
            JoinRoomRequest model_join_room;
            NetHelp.RecvData<JoinRoomRequest>(datas, out model_join_room);
            Guid RoooId = new Guid(model_join_room.RoomId);

            Log.Debug("房间id：{0}", RoooId.ToString());

            var player = GrainClient.GrainFactory.GetGrain<IPlayerGrain>(client.t.playerId);
            GameState state = await player.JoinGame(RoooId);

            ClientResult result = new ClientResult();
            result.Result = true;
            client.Send<ClientResult>(Op.Client.JoinRoom, result);
        }
    }
}
