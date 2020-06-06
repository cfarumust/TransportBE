using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TransportBE.Models.DataOperators;

namespace TransportBE.Services
{
    public interface IcommandText {

        string GetPUsers { get; }
        string PostOrder { get; }
        string GetPUserById { get; }
        string GetShipperByUserName { get; }
        string RegisterShipper { get; }
        string LoginShipper { get; }
    }
    public class CommandText : IcommandText
    {
        public string GetPUsers =>"select * from PUSERS";
        public string PostOrder => "Insert Into  ORDERS ( DTPICKUPDATE, DTDROPDATE, NCLIENTID, SADDRESSPICKUP, SADDRESSDROP, NBOXID, NBOXCOUNT, NDISTANCE, NPICKUPLAT, NPICKUPLONG, NDROPLAT, NDROPLONG) " +
            "OUTPUT INSERTED.NORDERID " +
            "Values ( @DTPICKUPDATE, @DTDROPDATE, @NCLIENTID, @sAddressPickUp, @sAddressDrop, @NBOXID, @NBOXCOUNT, @NDISTANCE, @NPICKUPLAT, @NPICKUPLONG, @NDROPLAT, @NDROPLONG ); SELECT CAST(SCOPE_IDENTITY() as int)";

        public string PostLoad => "Insert into Loads (NISMULTIVEHICLELOAD, NLEGID, SADDRESSPICKUP, SADDRESSDROP, NBOXCOUNT, NBOXID, NORDERID, SGRIDIDPICKUP, SGRIDIDDROP )" +
            " Values (@NISMULTIVEHICLELOAD, @NLEGID, @SADDRESSPICKUP, @SADDRESSDROP, @NBOXCOUNT, @NBOXID, @NORDERID, @SGRIDIDPICKUP, @SGRIDIDDROP )";
        public string GetPUserById => "select * from PUSERS where NUSERID=@NUSERID";

        public string GetLoadsByOrderId => "select NLOADID, SADDRESSPICKUP, SADDRESSDROP, " +
                                           "NBOXCOUNT, NBOXID, NPICKUPLAT, NPICKUPLONG, NORDERID, " +
                                           "NDROPLAT,NDROPLONG, SSTATUSID ,SGRIDIDPICKUP ," +
                                           "SGRIDIDDROP,NLEGID,NISMULTIVEHICLELOAD " + "FROM LOADS Where NORDERID = @NORDERID";

        public string GetAvailableLoads => "select NLOADID, SADDRESSPICKUP, SADDRESSDROP, " +
                                           "NBOXCOUNT, NBOXID, NPICKUPLAT, NPICKUPLONG, NORDERID, " +
                                           "NDROPLAT,NDROPLONG, SSTATUSID " +
                                           "FROM LOADS Where SSTATUSID = '2000'";

        public string AssignLoad => "update LOADS set SSTATUSID = '2001', NSHIPPERID = @NSHIPPERID where NLOADID = @NLOADID and SSTATUSID = '2000'";
        public string GetShipperByUserName => "select * from SHIPPERS where SUSERNAME = @SUSERNAME";

        public string LoginShipper => "select SUSERNAME, SPASSWORD, NSHIPPERID from SHIPPERS where SUSERNAME = @SUSERNAME and SPASSWORD=@SPASSWORD";

        public string LoginClient => "select SUSERNAME, SPASSWORD, NCLIENTID from CLIENTS where SUSERNAME = @SUSERNAME and SPASSWORD=@SPASSWORD";

        public string RegisterShipper => "Insert Into SHIPPER (SNAME, SADDRESS, SPHONE, SEMAIL, SUSERNAME, SPASSWORD, NVEHICLEID)" +
            "Values(@SNAME, @SADDRESS,@SPHONE, @SEMAIL, @SUSERNAME, @SPASSWORD, @NVEHICLEID)";

        public string GetOrdersByClient => "select NORDERID, SADDRESSPICKUP, SADDRESSDROP, NBOXID, NBOXCOUNT, DTPICKUPDATE, " +
                                                   "DTDROPDATE, SSTATUSID, NDISTANCE " +
                                                   "from Orders where NCLIENTID = @NCLIENTID order by " +
                                                   "DTORDEREDON desc";
    }
}
