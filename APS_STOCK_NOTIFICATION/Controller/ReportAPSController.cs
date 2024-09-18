using APS_STOCK_NOTIFICATION.Model;
using EKANBAN.Service;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APS_STOCK_NOTIFICATION.Controller
{
    internal class ReportAPSController
    {
        private ConnectDB oCOnnSCM = new ConnectDB("DBSCM");

        public List<DataIN_OUT_Report_ALL> getAPSReport()
        {
            List<DataIN_OUT_Report_ALL> dataIN_OUT_ALL_List = new List<DataIN_OUT_Report_ALL>();




            Dictionary<string, string[]> part_types_dict = new Dictionary<string, string[]>()

            {
                { "BODY", new string[] {"BODY" } },
                { "TOP", new string[] { "CASING TOP ASSY"}},
                { "BOTTOM", new string[] { "CASING BOTTOM ASSY" }},
                { "FS", new string[] { "FIXED SCROLL" }},
                { "OS", new string[] { "ORBITING SCROLL" } },
                { "CS", new string[] { "CRANK SHAFT" } },
                { "HS", new string[] { "HOUSING SCROLL" } },
                { "LW", new string[] { "LOWER" } },

                { "STATOR", new string[] { "STATOR" } },
                { "ROTOR", new string[] { "ROTOR" } }



            };





            foreach (var item in part_types_dict)
            {
                List<DataIN_OUT_Report_BY_TYPE> dataIN_OUT_Reports = new List<DataIN_OUT_Report_BY_TYPE>();


                DataIN_OUT_Report_ALL dataIN_OUT_Report_ALL = new DataIN_OUT_Report_ALL();
                dataIN_OUT_Report_ALL.part_type = item.Key + " (" + item.Value[0] + ")";

                SqlCommand sql_select_overall_Labels = new SqlCommand();
                sql_select_overall_Labels.CommandText = $@"	

                       SELECT ts.YMD,ts.SHIFT,ts.WCNO,ts.PARTNO,ts.CM,stock.PartDesc,models,part_desc,
                     ISNULL((SELECT SUM(TransQty) FROM EKB_WIP_PART_STOCK_TRANSACTION where TransType = 'IN' and SHIFT = @shift and YMD = @ymd and WCNO = '904' and  PARTNO = ts.PARTNO and CM = ts.CM ),0)  IN_STOCK
                     ,ISNULL((SELECT SUM(TransQty) FROM EKB_WIP_PART_STOCK_TRANSACTION where TransType = 'OUT' and SHIFT =@shift and YMD = @ymd and WCNO = '904' and PARTNO = ts.PARTNO and CM = ts.CM  ),0)  OUT_STOCK
                    ,stock.BAL BAL_STOCK, ISNULL(logs.NOTE,'-') REMARK , CASE WHEN logs.NOTE != '-' THEN 'TRUE' ELSE 'FALSE' END IS_REMARK
                   FROM EKB_WIP_PART_STOCK_TRANSACTION ts
                   LEFT JOIN (select STRING_AGG(model,',' ) models, part_desc, partno, cm from(
				                select distinct substring([DESCRIPTION],1,6) model, note part_desc, ref2 partno, ref3 cm
				                from DictMstr d where DICT_TYPE = 'PART_SET_OUT' and DICT_STATUS = 'ACTIVE' 
				                ) t1 
			                group by part_desc, partno, cm  
			                 ) m on m.partno = ts.PARTNO and m.cm = ts.CM
                   INNER JOIN EKB_WIP_PART_STOCK stock on stock.PARTNO = ts.PARTNO and stock.CM = ts.CM and stock.YM = @ym and stock.WCNO = '904'
                   LEFT JOIN DICT_SYSTEM_LOGS logs on logs.REF_CODE = ts.YMD and logs.DESCRIPTION = ts.SHIFT and logs.REF1 = ts.WCNO and logs.REF2 = ts.PARTNO and
			                logs.REF3 = ts.CM and logs.REF4 = models and logs.REF5 = part_desc and DICT_SYSTEM = 'APS_WIP_STOCK' and DICT_TYPE = 'FAC2_SCR' and DICT_STATUS = 'ACTIVE'
                   where ts.YMD = @ymd and SHIFT =@shift and ts.WCNO = '904' and CreateBy not in ('BATCH-PICKLIST','BATCH') and part_desc IN(@parttype) and  stock.BAL  < 0
                   GROUP BY ts.YMD,ts.SHIFT,ts.WCNO,ts.PARTNO,ts.CM,stock.PartDesc,stock.BAL,models,part_desc,logs.NOTE
                   order by BAL_STOCK";

                //sql_select_overall_Labels.Parameters.Add(new SqlParameter("@ym", DateTime.Now.AddHours(-8).ToString("yyyyMM")));
                //sql_select_overall_Labels.Parameters.Add(new SqlParameter("@ymd", DateTime.Now.AddHours(-8).ToString("yyyyMMdd")));
                //sql_select_overall_Labels.Parameters.Add(new SqlParameter("@shift", DateTime.Now.AddHours(-8).Hour >= 12 ? "N" : "D"));
                sql_select_overall_Labels.Parameters.Add(new SqlParameter("@ym", "202409"));
                sql_select_overall_Labels.Parameters.Add(new SqlParameter("@ymd", "20240917"));
                sql_select_overall_Labels.Parameters.Add(new SqlParameter("@shift", "N"));
                sql_select_overall_Labels.Parameters.Add(new SqlParameter("@parttype", item.Key));
                DataTable dtOverAll = oCOnnSCM.Query(sql_select_overall_Labels);


                if (dtOverAll.Rows.Count > 0)
                {
                    foreach (DataRow drAll in dtOverAll.Rows)
                    {
                        DataIN_OUT_Report_BY_TYPE dataIN_OUT_Report = new DataIN_OUT_Report_BY_TYPE();
                        dataIN_OUT_Report.ymd = drAll["YMD"].ToString();
                        dataIN_OUT_Report.shift = drAll["SHIFT"].ToString();
                        dataIN_OUT_Report.wcno = drAll["WCNO"].ToString();
                        dataIN_OUT_Report.partno = drAll["PARTNO"].ToString();
                        dataIN_OUT_Report.cm = drAll["cm"].ToString();
                        dataIN_OUT_Report.partDesc = drAll["part_desc"].ToString();
                        dataIN_OUT_Report.model = drAll["models"].ToString();
                        dataIN_OUT_Report.bal_stock = Convert.ToDecimal(drAll["BAL_STOCK"]);
                        dataIN_OUT_Report.remark = drAll["REMARK"].ToString();
                        dataIN_OUT_Report.remark_stauts = drAll["IS_REMARK"].ToString();


                        //dataIN_OUT_Report.in_stock = Convert.ToDecimal(drAll["OUT_STOCK"]) < 0 ? Convert.ToDecimal(drAll["IN_STOCK"]) + Math.Abs(Convert.ToDecimal(drAll["OUT_STOCK"])) : Convert.ToDecimal(drAll["IN_STOCK"]);
                        //dataIN_OUT_Report.out_stock = Convert.ToDecimal(drAll["OUT_STOCK"]) > 0 ? Convert.ToDecimal(drAll["OUT_STOCK"]) : 0;




                        //dataIN_OUT_Report.shift_bal_stock = (dataIN_OUT_Report.shift_lbal_stock + dataIN_OUT_Report.in_stock) - dataIN_OUT_Report.out_stock;
                        //dataIN_OUT_Report.status = findStatus(dataIN_OUT_Report.shift_lbal_stock, dataIN_OUT_Report.in_stock, dataIN_OUT_Report.out_stock, dataIN_OUT_Report.shift_bal_stock);
                        dataIN_OUT_Reports.Add(dataIN_OUT_Report);



                    }



                    dataIN_OUT_Report_ALL.reportAll = dataIN_OUT_Reports;

                    dataIN_OUT_ALL_List.Add(dataIN_OUT_Report_ALL);
                }
               
            }
            return dataIN_OUT_ALL_List;
        }

        //private string findStatus(decimal lbal, decimal in_stock, decimal out_stock, decimal bal)
        //{
        //    string status = "";
        //    decimal percentRangeINOUT = 0;
        //    if (in_stock != 0 && out_stock != 0)
        //    {
        //        percentRangeINOUT = ((Math.Abs(in_stock - out_stock) / (Math.Abs(in_stock + out_stock) / 2))) * 100;


        //    }


        //    if ((in_stock > 0 && out_stock == 0) && lbal > 0 && bal > 0)
        //    {
        //        status = "มียอด IN ไม่มียอด OUT";
        //    }
        //    else if ((in_stock == 0 && out_stock > 0) && lbal > 0 && bal > 0)
        //    {
        //        status = "มียอด OUT ไม่มียอด IN";
        //    }
        //    else if (lbal < 0 && bal >= 0)
        //    {
        //        status = "SHIFT LBAL (A) < 0";
        //    }
        //    else if (bal < 0 && lbal >= 0)
        //    {
        //        status = "SHIFT BAL (D) < 0";
        //    }
        //    else if (lbal < 0 && bal < 0)
        //    {
        //        status = "LBAL (A) และ BAL (D) < 0";
        //    }
        //    else if ((percentRangeINOUT >= 0 && percentRangeINOUT <= 50) && bal >= 0 && lbal >= 0)
        //    {
        //        status = "IN ไม่เท่ากับ OUT (0-50%)";
        //    }
        //    else if (percentRangeINOUT > 50 && percentRangeINOUT <= 200 && bal >= 0 && lbal >= 0)
        //    {
        //        status = "IN ไม่เท่ากับ OUT (50-200%)";
        //    }
        //    else if (percentRangeINOUT > 200 && bal > 0 && lbal > 0)
        //    {
        //        status = "IN ไม่เท่ากับ OUT (>200%)";
        //    }
        //    else
        //    {
        //        status = "ปกติ";
        //    }

        //    return status;

        //}
    }
}
