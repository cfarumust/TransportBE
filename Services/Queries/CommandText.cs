using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TransportBE.Models.DataOperators;

namespace TransportBE.Services
{
    public interface IcommandText {

        
        string PostOrder { get; }
       
        string CheckShipperUsernameExists { get; }
        string RegisterShipper { get; }
        string LoginShipper { get; }
        string PostLoad { get; }

        string GetLoadsByOrderId { get; }
    }
    public class CommandText : IcommandText
    {
        public string GetPUsers =>"select * from PUSERS";
        public string PostOrder => "Insert Into  ORDERS ( DTPICKUPDATE, DTDROPDATE, NCLIENTID, SADDRESSPICKUP, " +
                                   "SADDRESSDROP, NBOXID, NBOXCOUNT, NDISTANCE, NPICKUPLAT, NPICKUPLONG, NDROPLAT, NDROPLONG) " +
                                   "OUTPUT INSERTED.NORDERID " +
                                   "Values ( @DTPICKUPDATE, @DTDROPDATE, @NCLIENTID, @sAddressPickUp, @sAddressDrop," +
                                   "@NBOXID, @NBOXCOUNT, @NDISTANCE, @NPICKUPLAT, @NPICKUPLONG, @NDROPLAT, @NDROPLONG ); SELECT CAST(SCOPE_IDENTITY() as int)";

        public string PostLoad => "Insert into Loads (NISMULTIVEHICLELOAD, NLEGID, SADDRESSPICKUP, SADDRESSDROP, NBOXCOUNT, NBOXID, NORDERID )" +
             "OUTPUT INSERTED.NLOADID " +
            " Values (@NISMULTIVEHICLELOAD, @NLEGID, @SADDRESSPICKUP, @SADDRESSDROP, @NBOXCOUNT, @NBOXID, @NORDERID ); SELECT CAST(SCOPE_IDENTITY() as int)";
        public string PostWaypoint => "Insert into Waypoints (NLOADID, NORDERID, NLEGID, NLAT, NLONG)" +
            " Values (@NLOADID, @NORDERID, @NLEGID, @NLAT, @NLONG) ";

        public string GetOrderRouteWithWayPoints => "select NLAT, NLONG, NLEGID from Waypoints where NORDERID = @NORDERID order by NLEGID asc";

                
        public string GetLoadsByOrderId => "select NLOADID, SADDRESSPICKUP, SADDRESSDROP, " +
                                           "NBOXCOUNT, NBOXID, NPICKUPLAT, NPICKUPLONG, NORDERID, " +
                                           "NDROPLAT,NDROPLONG, SSTATUSID " + "FROM LOADS Where NORDERID = @NORDERID";

        public string GetLoadsByShipperId => "select NLOADID, SADDRESSPICKUP, SADDRESSDROP, " +
                                           "NBOXCOUNT, NBOXID, NPICKUPLAT, NPICKUPLONG, NORDERID, " +
                                           "NDROPLAT,NDROPLONG, SSTATUSID " + "FROM LOADS Where NSHIPPERID = @NSHIPPERID";

        public string GetAvailableLoads => "select NLOADID, SADDRESSPICKUP, SADDRESSDROP, " +
                                           "NBOXCOUNT, NBOXID, NPICKUPLAT, NPICKUPLONG, NORDERID, " +
                                           "NDROPLAT,NDROPLONG " +
                                           "FROM LOADS Where SSTATUSID = '2000'";

        public string AssignLoad => " update LOADS set SSTATUSID = '2001', NSHIPPERID = @NSHIPPERID  " +
                                    " OUTPUT INSERTED.NLOADID " +
                                    " where NLOADID = @NLOADID and SSTATUSID = '2000'; " +
                                    " SELECT CAST(SCOPE_IDENTITY() as int)";
        public string SetLoadStatusToDelivered => " update LOADS set SSTATUSID = '2003' " +
                                    " OUTPUT INSERTED.NLOADID " +
                                    " where NLOADID = @NLOADID and SSTATUSID = '2001' and NSHIPPERID= @NSHIPPERID; " +
                                    " SELECT CAST(SCOPE_IDENTITY() as int)";
        public string CheckShipperUsernameExists => "select * from SHIPPERS where SUSERNAME = @SUSERNAME";

        public string ClientRegister => "Insert Into CLIENTS (SNAME, SADDRESS, SPHONE, SEMAIL, SUSERNAME, SPASSWORD)" +
                                        "Values(@SNAME, @SADDRESS,@SPHONE, @SEMAIL, @SUSERNAME, @SPASSWORD)";
        public string CheckClientUsernameExists => "select * from CLIENTS where SUSERNAME = @SUSERNAME";
        public string LoginShipper => "select SUSERNAME, SPASSWORD, NSHIPPERID from SHIPPERS where SUSERNAME = @SUSERNAME and SPASSWORD=@SPASSWORD";

        public string LoginClient => "select SUSERNAME, SPASSWORD, NCLIENTID from CLIENTS where SUSERNAME = @SUSERNAME and SPASSWORD=@SPASSWORD";

        public string RegisterShipper => "Insert Into SHIPPERS (SNAME, SADDRESS, SPHONE, SEMAIL, SUSERNAME, SPASSWORD) " +
            "Values(@SNAME, @SADDRESS,@SPHONE, @SEMAIL, @SUSERNAME, @SPASSWORD )";

        public string GetOrdersByClient => "select NORDERID, SADDRESSPICKUP, SADDRESSDROP, NBOXID, NBOXCOUNT, DTPICKUPDATE, DTORDEREDON,  " +
                                                   "DTDROPDATE, SSTATUSID, NDISTANCE " +
                                                   "from Orders where NCLIENTID = @NCLIENTID order by " +
                                                   "DTORDEREDON desc";
    }
}
