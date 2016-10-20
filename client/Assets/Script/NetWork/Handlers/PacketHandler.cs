using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using google.protobuf;
public class PacketHandler : MonoBehaviour
{
    private delegate void PacketHandlerFunc(byte[] datas);
    private class PacketHandlerAttribute : Attribute
    {
        public int[] Ops { get; protected set; }

        public PacketHandlerAttribute(params int[] ops)
        {
            this.Ops = ops;
        }
    }
    private Dictionary<int, PacketHandlerFunc> _handlers = new Dictionary<int, PacketHandlerFunc>();
    void Start()
    {
        foreach (var method in this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            foreach (PacketHandlerAttribute attr in method.GetCustomAttributes(typeof(PacketHandlerAttribute), false))
            {
                var del = (PacketHandlerFunc)Delegate.CreateDelegate(typeof(PacketHandlerFunc), this, method);
                foreach (var op in attr.Ops)
                    _handlers[op] = del;
            }
        }
    }

    void Update()
    {
        HandlePackets();

        //if (regionLoadFailMsg != null && regionLoadFailMsg.Result != MsgBoxResult.Pending)
        //{
        //    regionLoadFailMsg = null;
        //    Connection.Client.Disconnect();
        //    SceneManager.LoadScene("Login");
        //}
    }

    private void HandlePackets()
    {
        if (Connection.Client.State != ConnectionState.Connected)
            return;

        var packets = Connection.Client.GetPacketsFromQueue();
        foreach (var packet in packets)
        {
            PacketHandlerFunc handler;
            if (!_handlers.TryGetValue(packet.Op, out handler))
            {
                Debug.LogFormat("Unhandled packet: {0:X4} ({1})", packet.Op, Op.GetName(packet.Op));
                continue;
            }
            handler(packet.datas);
        }
    }

    [PacketHandler(Op.Client.Login)]
    private void LoginR(byte[] packet)
    {
        ClientResult result;
        NetHelp.RecvData<ClientResult>(packet, out result);
        Debug.Log("登陆结果：" + result.Result);
    }


    //// Ident -> Login
    //[PacketHandler(Op.ClientIdentR)]
    //private void ClientIdentR(byte[] packet)
    //{
    //    var form = GetComponentIn<LoginForm>("FrmLogin");
    //    if (form == null || form.State != LoginState.Ident)
    //    {
    //        Debug.Log("Received ClientIdentR outside of login or in incorrect state.");
    //        return;
    //    }

    //    var username = form.TxtUsername.text;
    //    var password = form.TxtPassword.text;

    //    var md5 = MD5.Create();
    //    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
    //    var sHash = BitConverter.ToString(hash).Replace("-", "");
    //    var sbHash = Encoding.UTF8.GetBytes(sHash);

    //    packet = new Packet(Op.Login, 0);
    //    packet.PutByte(12); // Normal login type
    //    packet.PutString(username);
    //    packet.PutBin(sbHash);
    //    packet.PutBin();
    //    packet.PutInt(0);
    //    packet.PutInt(0);
    //    packet.PutString(Connection.Client.GetLocalIp());
    //    Connection.Client.Send(packet);

    //    form.State = LoginState.Login;
    //}

    //// Login -> LoggedIn | Ready
    //[PacketHandler(Op.LoginR)]
    //private void LoginR(byte[] packet)
    //{
    //    var form = GetComponentIn<LoginForm>("FrmLogin");
    //    if (form == null || form.State != LoginState.Login)
    //    {
    //        Debug.Log("Received LoginR outside of login or in incorrect state.");
    //        return;
    //    }

    //    var result = (LoginResult)packet.GetByte();
    //    if (result != LoginResult.Success)
    //    {
    //        switch (result)
    //        {
    //            case LoginResult.Message:
    //                var unkInt1 = packet.GetInt();
    //                var unkInt2 = packet.GetInt();
    //                var message = packet.GetString();
    //                form.ResetForm(message);
    //                break;

    //            case LoginResult.IdOrPassIncorrect: form.ResetForm("The username or password is incorrect."); break;
    //            case LoginResult.SecondaryFail: form.ResetForm("The secondary password is incorrect."); break;
    //            case LoginResult.AlreadyLoggedIn: form.ResetForm("This account is already logged in."); break;

    //            default: form.ResetForm("Login failed."); break;
    //        }

    //        return;
    //    }

    //    // Parse
    //    var accountName = packet.GetString();
    //    var accountName2 = packet.GetString();
    //    var sessionKey = packet.GetLong();
    //    var unkByte1 = packet.GetByte();
    //    var servers = packet.GetServerList();
    //    var lastLogin = packet.GetDateTime();
    //    var lastLogout = packet.GetDateTime();
    //    var unkInt3 = packet.GetInt();
    //    var unkByte3 = packet.GetByte();
    //    var unkByte4 = packet.GetByte();
    //    var unkInt4 = packet.GetInt();
    //    var unkByte5 = packet.GetByte();

    //    var naosSupport = packet.GetBool();
    //    var naosSupportExpiration = packet.GetDateTime();
    //    var extraStorage = packet.GetBool();
    //    var extraStorageExpiration = packet.GetDateTime();
    //    var advancedPlay = packet.GetBool();
    //    var advancedPlayExpiration = packet.GetDateTime();

    //    var unkByte6 = packet.GetByte();
    //    var unkByte7 = packet.GetByte();

    //    var inventoryPlus = packet.GetBool();
    //    var inventoryPlusExpiration = packet.GetDateTime();
    //    var premiumService = packet.GetBool();
    //    var premiumServiceExpiration = packet.GetDateTime();
    //    var vipService = packet.GetBool();
    //    var vipServiceExpiration = packet.GetDateTime();
    //    var unkPremium1 = packet.GetBool();
    //    var unkPremium1Expiration = packet.GetDateTime();
    //    var unkPremium2 = packet.GetBool();
    //    var unkPremium2Expiration = packet.GetDateTime();

    //    var unkByte8 = packet.GetByte();
    //    var pcCafe = packet.GetByte();
    //    var freeBeginnerService = packet.GetByte();
    //    var unkByte = packet.GetByte();

    //    var characters = new List<CharacterInfo>();
    //    var characterCount = packet.GetShort();
    //    for (int i = 0; i < characterCount; ++i)
    //    {
    //        var serverName = packet.GetString();
    //        var entityId = packet.GetLong();
    //        var characterName = packet.GetString();
    //        var deletionFlag = (DeletionFlag)packet.GetByte();
    //        var unkLong1 = packet.GetLong();
    //        var unkInt5 = packet.GetInt();
    //        var unkByte9 = packet.GetByte();
    //        var unkByte10 = packet.GetByte();
    //        var unkByte11 = packet.GetByte();

    //        var character = new CharacterInfo();
    //        character.Server = serverName;
    //        character.EntityId = entityId;
    //        character.Name = characterName;
    //        character.DeletionFlag = deletionFlag;

    //        characters.Add(character);
    //    }

    //    var pets = new List<CharacterInfo>();
    //    var petCount = packet.GetShort();
    //    for (int i = 0; i < petCount; ++i)
    //    {
    //        var serverName = packet.GetString();
    //        var entityId = packet.GetLong();
    //        var characterName = packet.GetString();
    //        var deletionFlag = (DeletionFlag)packet.GetByte();
    //        var unkLong2 = packet.GetLong();
    //        var race = packet.GetInt();
    //        var unkLong3 = packet.GetLong();
    //        var unkLong4 = packet.GetLong();
    //        var unkInt6 = packet.GetInt();
    //        var unkByte12 = packet.GetByte();

    //        var character = new CharacterInfo();
    //        character.Server = serverName;
    //        character.EntityId = entityId;
    //        character.Name = characterName;
    //        character.DeletionFlag = deletionFlag;

    //        characters.Add(character);
    //    }

    //    // Set
    //    Connection.AccountName = accountName;
    //    Connection.SessionKey = sessionKey;
    //    Connection.Servers.Clear();
    //    Connection.Servers.AddRange(servers);
    //    Connection.Characters.Clear();
    //    Connection.Characters.AddRange(characters);
    //    Connection.Characters.AddRange(pets);

    //    // Transition
    //    SceneManager.LoadScene("CharacterSelect");
    //    form.State = LoginState.LoggedIn;
    //}

    //[PacketHandler(Op.Walking, Op.Running)]
    //private void Moving(Packet packet)
    //{
    //    var fromX = packet.GetInt();
    //    var fromY = packet.GetInt();
    //    var toX = packet.GetInt();
    //    var toY = packet.GetInt();

    //    var walking = (packet.Op == Op.Walking);
    //    var x = toX / 100f;
    //    var y = toY / 100f;

    //    Entity entity;
    //    if (Connection.Entities.TryGetValue(packet.Id, out entity))
    //    {
    //        var controller = entity.Transform.GetComponent<CreatureController>();
    //        if (controller != null)
    //            controller.Move(new Vector3(x, 0, y), walking);
    //    }
    //}
}
