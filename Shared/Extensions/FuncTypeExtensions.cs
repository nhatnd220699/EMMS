using Shared.Enums;
using System;

namespace Shared.Extensions
{
    public static class FuncTypeExtensions
    {
        public static string ToCode(this FuncType funcType)
        {
            switch (funcType)
            {
                case FuncType.Get_Type:
                    return "Get_Type";
                case FuncType.Get_Line:
                    return "Get_Line";
                case FuncType.Get_Dept:
                    return "Get_Dept";
                case FuncType.Get_Equipment_List:
                    return "Get_Equipment_List";
                case FuncType.Get_Equipment_By_Month:
                    return "Get_Equipment_By_Month";
                case FuncType.Get_Equipment_By_Day:
                    return "Get_Equipment_By_Day";
                case FuncType.Upload_Equipment_By_SN:
                    return "Upload_Equipment_By_SN";
                case FuncType.Edit_Equipment_By_SN:
                    return "Edit_Equipment_By_SN";
                case FuncType.Approve_Equipment_By_SN:
                    return "Approve_Equipment_By_SN";
                default:
                    throw new ArgumentOutOfRangeException(nameof(funcType), funcType, null);
            }
        }

        public static FuncType FromCode(this string funcType)
        {
            switch (funcType)
            {
                case "Get_Type":
                    return FuncType.Get_Type;
                case "Get_Line":
                    return FuncType.Get_Line;
                case "Get_Dept":
                    return FuncType.Get_Dept;
                case "Get_Equipment_List":
                    return FuncType.Get_Equipment_List;
                case "Get_Equipment_By_Month":
                    return FuncType.Get_Equipment_By_Month;
                case "Get_Equipment_By_Day":
                    return FuncType.Get_Equipment_By_Day;
                case "Upload_Equipment_By_SN":
                    return FuncType.Upload_Equipment_By_SN;
                case "Edit_Equipment_By_SN":
                    return FuncType.Edit_Equipment_By_SN;
                case "Approve_Equipment_By_SN":
                    return FuncType.Approve_Equipment_By_SN;
                default:
                    throw new ArgumentException("Invalid FuncType", "code");
            }
        }
    }
}
