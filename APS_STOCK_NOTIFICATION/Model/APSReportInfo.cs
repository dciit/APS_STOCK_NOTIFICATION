using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APS_STOCK_NOTIFICATION.Model
{
    class APSReportInfo
    {


    }

    public class DataIN_OUT_Report_BY_TYPE
    {


        public string ymd { get; set; }

        public string shift { get; set; }

        public string wcno { get; set; }

        public string partno { get; set; }
        public string cm { get; set; }
        public string partDesc { get; set; }
        public string model { get; set; }

        public decimal shift_lbal_stock { get; set; }

        public decimal in_stock { get; set; }
        public decimal out_stock { get; set; }

        public decimal shift_bal_stock { get; set; }
        public decimal bal_stock { get; set; }

        public string remark { get; set; }

        public string remark_stauts { get; set; }
        public string status { get; set; }


    }

    public class DataIN_OUT_Report_ALL
    {
        public string part_type { get; set; }
        public List<DataIN_OUT_Report_BY_TYPE> reportAll { get; set; }

    }
}
