// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Comm.Util;

namespace Comm.Network
{
    /// <summary>
    /// Packet handler manager base class.
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class PacketHandlerManager<TClient> where TClient : BaseClient
    {
        public delegate void PacketHandlerFunc(TClient client, int command,byte[] datas);

        private Dictionary<int, PacketHandlerFunc> _handlers;

        protected PacketHandlerManager()
        {
            _handlers = new Dictionary<int, PacketHandlerFunc>();
        }

        /// <summary>
        /// Adds and/or overwrites handler.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="handler"></param>
        public void Add(int op, PacketHandlerFunc handler)
        {
            _handlers[op] = handler;
        }

        /// <summary>
        /// Adds all methods with a Handler attribute.
        /// </summary>
        public void AutoLoad()
        {
            foreach (var method in this.GetType().GetMethods())
            {
                foreach (PacketHandlerAttribute attr in method.GetCustomAttributes(typeof(PacketHandlerAttribute), false))
                {
                    var del = (PacketHandlerFunc)Delegate.CreateDelegate(typeof(PacketHandlerFunc), this, method);
                    foreach (var op in attr.Ops)
                        this.Add(op, del);
                }
            }
        }

        /// <summary>
        /// Runs handler for packet's op, or logs it as unimplemented.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        public virtual void Handle(TClient client, int command, byte[] datas)
        {
            PacketHandlerFunc handler;
            if (!_handlers.TryGetValue(command, out handler))
            {
                this.UnknownPacket(client, command);
                return;
            }

            try
            {
                handler(client, command, datas);
            }
            catch (PacketElementTypeException ex)
            {
                Log.Error(
                    "PacketElementTypeException: " + ex.Message + Environment.NewLine +
                    ex.StackTrace + Environment.NewLine +
                    "Packet: " + Environment.NewLine +
                    command.ToString()
                );
            }
        }

        public virtual void UnknownPacket(TClient client, int command)
        {
            Log.Unimplemented("PacketHandlerManager: Handler for '{0:X4}', '{1}'.", command, Op.GetName(command));
        }
    }

    /// <summary>
    /// Methods having this attribute are registered as packet handlers,
    /// for the ops.
    /// </summary>
    public class PacketHandlerAttribute : Attribute
    {
        public int[] Ops { get; protected set; }

        public PacketHandlerAttribute(params int[] ops)
        {
            this.Ops = ops;
        }
    }

    public enum PacketElementType : byte
    {
        None = 0,
        Byte = 1,
        Short = 2,
        Int = 3,
        Long = 4,
        Float = 5,
        String = 6,
        Bin = 7,
    }

    /// <summary>
    /// An exception for when a value of the wrong type is read from Packet.
    /// </summary>
    public class PacketElementTypeException : Exception
    {
        public PacketElementTypeException(PacketElementType expected, PacketElementType actual)
            : base(string.Format("Expected {0}, got {1}.", expected, actual))
        {
        }
    }
}
