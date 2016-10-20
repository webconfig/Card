using System;
using System.Collections.Generic;
using System.Reflection;
public static class Op
{
    public static class Client
    {
        public const int Login = 0x10000001;
    }


    public static string GetName(int op)
    {
        foreach (var field in typeof(Op).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if ((int)field.GetValue(null) == op)
                return field.Name;
        }
        return "?";
    }
}
