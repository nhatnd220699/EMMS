using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Web;

namespace Application.Mapper
{
    public class FixtureMapper : IFixtureMapper
    {
        public FixtureDaily2Dto Map(HttpRequest request)
        {
            var model = new FixtureDaily2Dto
            {
                EQ_SN = request.Form["EQ_SN"],
                EQ_Name = request.Form["EQ_Name"],
                EQ_Model = request.Form["EQ_Model"],
                Line = request.Form["Line"],
                Date = DateTime.Parse(request.Form["Date"]),
                Shift = request.Form["Shift"],
                IsApproverMode = bool.Parse(request.Form["IsApproverMode"]),
                CheckItems = parseCheckItems(request)
            };

            return model;
        }

        public List<DailyItem2Dto> parseCheckItems(HttpRequest request)
        {
            var items = new List<DailyItem2Dto>();

            int index = 0;

            while (!string.IsNullOrEmpty(request.Form[$"CheckItems[{index}].MT_No"]))
            {
                var item = new DailyItem2Dto
                {
                    Number_Order = int.Parse(request.Form[$"CheckItems[{index}].Number_Order"]),
                    MT_No = request.Form[$"CheckItems[{index}].MT_No"],
                    MT_Item = request.Form[$"CheckItems[{index}].MT_Item"],
                    MT_Desc = request.Form[$"CheckItems[{index}].MT_Desc"],

                    Upload_Result = request.Form[$"CheckItems[{index}].Upload_Result"],
                    Approval_Result = request.Form[$"CheckItems[{index}].Approval_Result"],

                    Upload_ID = request.Form[$"CheckItems[{index}].Upload_ID"],
                    Approval_ID = request.Form[$"CheckItems[{index}].Approval_ID"],

                    Remark = request.Form[$"CheckItems[{index}].Remark"],

                    ExistingImagePaths = ParseExistsImageStringList(request, $"CheckItems[{index}].ExistingImagePaths")
                };

                items.Add(item);
                index++;
            }
            return items;
        }

        private List<string> ParseExistsImageStringList(HttpRequest request, string prefix)
        {
            var result = new List<string>();
            int i = 0;

            while (!string.IsNullOrEmpty(request.Form[$"{prefix}[{i}]"]))
            {
                result.Add(request.Form[$"{prefix}[{i}]"]);
                i++;
            }

            return result;
        }
    }
}
