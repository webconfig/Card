//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: ClientData.proto
namespace google.protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ClientLogin")]
  public partial class ClientLogin : global::ProtoBuf.IExtensible
  {
    public ClientLogin() {}
    
    private string _UserName;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"UserName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string UserName
    {
      get { return _UserName; }
      set { _UserName = value; }
    }
    private string _Password;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"Password", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string Password
    {
      get { return _Password; }
      set { _Password = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ClientResult")]
  public partial class ClientResult : global::ProtoBuf.IExtensible
  {
    public ClientResult() {}
    
    private bool _Result;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"Result", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public bool Result
    {
      get { return _Result; }
      set { _Result = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}