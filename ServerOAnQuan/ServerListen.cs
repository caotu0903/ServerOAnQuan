using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using System.IO;

namespace ServerOAnQuan
{
    public partial class ServerListen : Form
    {
        public SqlConnection _sqlConnection;
        public string sql;
        public DataSet dataSet;
        public SqlDataAdapter sqlDataAdapter;
        Thread StartListen;

        public ServerListen()
        {
            InitializeComponent();
        }

        public bool IsListen;
        public static Hashtable htClient = new Hashtable();
        public static Hashtable htGameRoom = new Hashtable();

        private void bt_StartListen_Click(object sender, EventArgs e)
        {
            if (bt_StartListen.Text == "Bắt đầu nhận kết nối")
            {
                IsListen = true;
                bt_StartListen.Enabled = false;
                tb_IP.ReadOnly = true;
                tb_Port.ReadOnly = true;
                InfoMessage("Start listening\r\n");
                StartListen = new Thread(new ThreadStart(ListenThread));
                StartListen.Start();

                _sqlConnection = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=OAnQuanGame;Integrated Security=True");
                _sqlConnection.Open();
            }
        }

        void ListenThread()
        {
            IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Parse(tb_IP.Text), int.Parse(tb_Port.Text));
            TcpListener tcpListener = new TcpListener(ipEndpoint);
            tcpListener.Start();
            TcpClient tcpClient = new TcpClient();
            while (IsListen)
            {
                tcpClient = tcpListener.AcceptTcpClient();
                Connection connection = new Connection(tcpClient, this);
            }


        }

        public delegate void SafeCallInfoMessage(string message);
        public void InfoMessage(string mess)
        {
            if (this.tb_Nofitication.InvokeRequired)
            {
                SafeCallInfoMessage delInfoMessage = new SafeCallInfoMessage(InfoMessage);
                this.tb_Nofitication.Invoke(delInfoMessage, new object[] { mess });
            }
            else
            {
                this.tb_Nofitication.AppendText(mess);
            }
        }

        public static void AddClient(TcpClient tcpClient, string accountname)
        {
            htClient.Add(accountname, tcpClient);
        }

        public static void DeleteClient(TcpClient tcpClient, string accountname)
        {
            htClient.Remove(accountname);
        }

        public static string EncryptPass(string password)
        {
            try
            {
                byte[] encData_byte = new byte[password.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }

        public string DecryptPass(string encodedData)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encodedData);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }

        public class Connection
        {
            public StreamReader _sr;
            public StreamWriter _sw;
            public TcpClient _client;
            public Thread _acceptClient;
            public string _receiveData;
            public string _sendData;
            public string _username;
            public string _accountname;
            public string _password;
            public string _stringmess;
            public ServerListen _frmServerListen;
            public string _sql;
            public DataTable _dataTable;
            public SqlDataReader _sqlDataReader;
            public SqlCommand _sqlCommand;
            public int _row;
            public bool _login = false;

            public Connection (TcpClient tcpClient, ServerListen frmServerListen)
            {
                _frmServerListen = frmServerListen;
                _client = tcpClient;
                _acceptClient = new Thread(AcceptClient);
                _acceptClient.Start();
                frmServerListen.InfoMessage("new client connect\r\n");
            }

            public void AcceptClient()
            {
                _sr = new StreamReader(_client.GetStream());
                _sw = new StreamWriter(_client.GetStream());
                _dataTable = new DataTable();
                _sqlCommand = new SqlCommand("select * from THONGTINTAIKHOAN", _frmServerListen._sqlConnection);

                try
                {
                    while (_client.Connected)
                    {
                        _receiveData = _sr.ReadLine();

                        if (_receiveData != null)
                        {
                            _receiveData = _frmServerListen.DecryptPass(_receiveData);
                            _frmServerListen.InfoMessage(_receiveData + "\r\n");
                            if (_receiveData.StartsWith("100"))
                            {
                                SignUp();
                            }
                            else if (_receiveData.StartsWith("101"))
                            {
                                Login();
                            }
                            else if (_receiveData.StartsWith("102"))
                            {
                                Logout();
                            }
                            else if (_receiveData.StartsWith("103"))
                            {
                                ChangeAccountName();
                            }
                            else if (_receiveData.StartsWith("104"))
                            {
                                ChangePassword();
                            }
                            else if (_receiveData.StartsWith("200"))
                            {
                                CreateRoom();
                            }
                            else if (_receiveData.StartsWith("201"))
                            {
                                JoinRoom();
                            }
                            else if (_receiveData.StartsWith("202"))
                            {
                                OutRoom();
                            }
                            else if (_receiveData.StartsWith("222"))
                            {
                                GetRoomList();
                            }
                            else if (_receiveData.StartsWith("300"))
                            {
                                StartGame();
                            }
                            else if (_receiveData.StartsWith("301"))
                            {
                                Ready();
                            }
                            else if (_receiveData.StartsWith("302"))
                            {
                                Unready();
                            }
                            else if (_receiveData.StartsWith("303"))
                            {
                                KickPlayer();
                            }
                            else if (_receiveData.StartsWith("310"))
                            {
                                GetRoomInfo();
                            }
                            else if (_receiveData.StartsWith("400"))
                            {
                                SendMove();
                            }
                            else if (_receiveData.StartsWith("401"))
                            {
                                OutGame();
                            }
                            else if (_receiveData.StartsWith("402"))
                            {
                                SendResult();
                            }
                            else if (_receiveData.StartsWith("500"))
                            {
                                GetRankList();
                            }
                        }
                        else
                        {
                            if (_login == true)
                            {
                                _login = false;
                                UpdateStatus(_username, 0);
                                DeleteClient(_client, _accountname);
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    if (!_client.Connected)
                    {
                        _login = false;
                        UpdateStatus(_username, 0);
                        DeleteClient(_client, _accountname);
                    }
                }
            }

            public void SignUp()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);
                _sql = "select USERNAME from THONGTINTAIKHOAN where USERNAME = '" + splitData[0] + "'";
                DoSQLSelectCommand();
                if (_dataTable.Rows.Count != 0)
                {
                    SendEncryptMessage("002", _sw); // User đã tồn tại
                }
                else 
                {
                    _sql = "select ACCOUNTNAME from THONGTINTAIKHOAN where ACCOUNTNAME = '" + splitData[1] + "'";
                    DoSQLSelectCommand();
                    if (_dataTable.Rows.Count != 0)
                    {
                        SendEncryptMessage("003", _sw); // Tên hiển thị đã tồn tại 
                    }
                    else
                    {
                        _sql = "set dateformat DMY ";
                        _sql += "insert into THONGTINTAIKHOAN (USERNAME, ACCOUNTNAME, EMAIL, PASSWORD, STATUS, CREATETIME) values (";
                        _sql += "'" + splitData[0] + "','" + splitData[1] + "','" + splitData[2] + "','" + splitData[3] + "', 0, getdate())";
                        _sqlCommand.CommandText = _sql;
                        int numberRowAffected = _sqlCommand.ExecuteNonQuery();
                        if (numberRowAffected == 1)
                        {
                            SendEncryptMessage("001", _sw); // Tạo User thành công
                        }
                        else
                        {
                            SendEncryptMessage("000", _sw); // Tạo User thất bại
                        }
                    }
                }
            }

            public void Login()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);
                _sql = "select USERNAME from THONGTINTAIKHOAN where USERNAME = '" + splitData[0] + "'";
                DoSQLSelectCommand();
                if (_dataTable.Rows.Count != 0)
                {
                    _sql = "select PASSWORD from THONGTINTAIKHOAN where USERNAME = '" + splitData[0] + "'";
                    DoSQLSelectCommand();
                    string PassInDatabase = _dataTable.Rows[0].ItemArray[0].ToString();
                    if (splitData[1] == PassInDatabase)
                    {
                        if (htClient.ContainsKey(splitData[0]) == true)
                        {
                            SendEncryptMessage("002", _sw); // User đang online
                        }
                        else
                        {
                            _sql = "select ACCOUNTNAME from THONGTINTAIKHOAN where USERNAME = '" + splitData[0] + "'";
                            DoSQLSelectCommand();
                            _accountname = _dataTable.Rows[0].ItemArray[0].ToString();

                            AddClient(_client, _accountname);
                            UpdateStatus(splitData[0], 1);
                            _login = true;
                            _username = splitData[0];

                            SendEncryptMessage("001" + _accountname, _sw); // Đăng nhập thành công
                        }
                    }
                    else
                    {
                        SendEncryptMessage("000", _sw); // Đăng nhập thất bại
                    }
                }
                else if (_row == 0)
                {
                    SendEncryptMessage("000", _sw); // Đăng nhập thất bại
                }
            }

            public void ChangeAccountName()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);
                _sql = "select ACCOUNTNAME from THONGTINTAIKHOAN where ACCOUNTNAME = '" + splitData[0] + "'";
                DoSQLSelectCommand();
                if (_dataTable.Rows.Count != 0)
                {
                    SendEncryptMessage("103" + "003", _sw); // Tên hiển thị đã tồn tại 
                }
                else
                {
                    _sql = "update THONGTINTAIKHOAN set ACCOUNTNAME = '" + splitData[0] + "' where USERNAME = '" + _username + "'";
                    _sqlCommand.CommandText = _sql;
                    _sqlCommand.ExecuteNonQuery();
                    DeleteClient(_client, _accountname);
                    _accountname = splitData[0];
                    AddClient(_client, _accountname);
                    SendEncryptMessage("103" + "001", _sw);
                }
            }

            public void ChangePassword()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);
                _sql = "select PASSWORD from THONGTINTAIKHOAN where USERNAME = '" + _username + "'";
                DoSQLSelectCommand();
                string PassInDatabase = _dataTable.Rows[0].ItemArray[0].ToString();
                if (splitData[0] == PassInDatabase)
                {
                    _sql = "update THONGTINTAIKHOAN set PASSWORD = '" + splitData[1] + "' where USERNAME = '" + _username + "'";
                    _sqlCommand.CommandText = _sql;
                    _sqlCommand.ExecuteNonQuery();
                    SendEncryptMessage("104" + "001", _sw); // Đổi mật khẩu thành công
                }
                else
                {
                    SendEncryptMessage("104" + "000", _sw); // Mật khẩu hiện tại sai
                }
            }

            public void GetRoomList()
            {
                string getRoomMessage = "222";
                string[] array = new string[htGameRoom.Count];
                htGameRoom.Keys.CopyTo(array, 0);
                foreach (var roomName in array)
                {
                    ArrayList arrayList = (ArrayList)htGameRoom[roomName];
                    getRoomMessage += roomName + "/**/" + arrayList.Count + "/**/" + arrayList[0] + "/**/";
                }
                _frmServerListen.InfoMessage(getRoomMessage + "\r\n");
                SendEncryptMessage(getRoomMessage, _sw);
            }

            public void CreateRoom()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);
                int i = 0;
                while (i < htGameRoom.Count)
                {
                    if (!htGameRoom.Contains("OAQ-" + i))
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
                ArrayList arrayList = new ArrayList();
                arrayList.Add(_accountname);
                htGameRoom.Add("OAQ-" + i, arrayList);

                SendEncryptMessage("001" + "OAQ-" + i, _sw); // Tạo phòng thành công
            }

            public void JoinRoom()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);
                if (htGameRoom.ContainsKey(splitData[0]))
                {
                    if (GetNumberPlayerInRoom(splitData[0]) == 1)
                    {
                        ArrayList arrayList = (ArrayList)htGameRoom[splitData[0]];
                        arrayList.Add(_accountname);
                        SendEncryptMessage("210", _sw); // Vào phòng thành công
                        TcpClient tcpClient = (TcpClient)htClient[arrayList[0]];
                        StreamWriter sw = new StreamWriter(tcpClient.GetStream());

                        string getRoomInfoMessage = "310";

                        if (arrayList.Count == 2)
                        {
                            getRoomInfoMessage += splitData[0] + "/**/" + arrayList[0] + "/**/" + arrayList[1] + "/**/" + "false/**/";
                        }

                        SendEncryptMessage(getRoomInfoMessage, sw);

                    }
                    else if (GetNumberPlayerInRoom(splitData[0]) == 2)
                    {
                        SendEncryptMessage("221", _sw); // Phòng đã đầy
                    }
                }
                else
                {
                    SendEncryptMessage("220", _sw); // Phòng không tồn tại
                }
            }

            public void OutRoom()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);
                ArrayList arrayList = (ArrayList)htGameRoom[splitData[0]];
                if (arrayList.Count == 1)
                {
                    arrayList.Remove(_accountname);
                    htGameRoom.Remove(splitData[0]);
                }
                else if (arrayList.Count == 2) {

                    arrayList.Remove(_accountname);

                    TcpClient tcpClient = (TcpClient)htClient[arrayList[0]];
                    StreamWriter sw = new StreamWriter(tcpClient.GetStream());

                    string getRoomInfoMessage = "310";

                    getRoomInfoMessage += splitData[0] + "/**/" + arrayList[0] + "/**/" + "unknown" + "/**/" + "false/**/";

                    SendEncryptMessage(getRoomInfoMessage, sw);
                }
                SendEncryptMessage("001", _sw);
            }

            public void KickPlayer()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);

                string getRoomInfoMessage = "310";

                ArrayList arrayList = (ArrayList)htGameRoom[splitData[0]];

                try
                {
                    TcpClient tcpClient = (TcpClient)htClient[arrayList[1]];
                    StreamWriter sw = new StreamWriter(tcpClient.GetStream());

                    SendEncryptMessage("303", sw); // Gửi thông tin kick tới player 2 

                    arrayList.Remove(arrayList[1]);
                }
                catch
                {
                    
                }

                getRoomInfoMessage += splitData[0] + "/**/" + arrayList[0] + "/**/" + "unknown" + "/**/" + "false/**/";
                SendEncryptMessage(getRoomInfoMessage, _sw); // Gửi thông tin kick thành công tới host
            }

            public void SendMove()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);
                ArrayList arrayList = (ArrayList)htGameRoom[splitData[0]];
                foreach (string user in arrayList)
                {
                    if (user == _accountname)
                    {
                        string moveMessage = "400" + splitData[1] + "/**/" + splitData[2] + "/**/" + splitData[3] + "/**/";
                        SendEncryptMessage(moveMessage, _sw);
                    }
                    else
                    {
                        int oDuocChon = Int32.Parse(splitData[1]) + 6;
                        string moveMessage = "400" + oDuocChon.ToString() + "/**/" + splitData[2] + "/**/" + splitData[3] + "/**/";
                        TcpClient tcpClient = (TcpClient)htClient[user];
                        StreamWriter sw = new StreamWriter(tcpClient.GetStream());
                        SendEncryptMessage(moveMessage, sw);
                    }
                }
            }

            public void StartGame()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);

                ArrayList arrayList = (ArrayList)htGameRoom[splitData[0]];

                SendToAllPlayerInRoom(splitData[0], "300");
            }

            public void OutGame()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);

                _sql = "select POINT from BANGXEPHANG where USERNAME = '" + _username + "' and GAMECODE = '" + splitData[0] + "'";
                DoSQLSelectCommand();

                int point = -10;

                if (_dataTable.Rows.Count == 0)
                {
                    _sql = "insert into BANGXEPHANG (USERNAME, GAMECODE, POINT) values (";
                    _sql += "'" + _username + "','" + splitData[0] + "'," + 0 + ")";
                    _sqlCommand.CommandText = _sql;
                    _sqlCommand.ExecuteNonQuery();

                }
                else
                {
                    if ((int)_dataTable.Rows[0].ItemArray[0] >= 10)
                    {
                        _sql = "update BANGXEPHANG set POINT =  POINT + " + point + " where USERNAME = '" + _username + "' and GAMECODE = '" + splitData[0] + "'";
                        _sqlCommand.CommandText = _sql;
                        _sqlCommand.ExecuteNonQuery();
                    }
                }

                _sql = "select USERNAME from THONGTINTAIKHOAN where ACCOUNTNAME = '" + splitData[2] + "'";
                DoSQLSelectCommand();

                string usernamePlayer2 = (string)_dataTable.Rows[0].ItemArray[0];

                _sql = "select POINT from BANGXEPHANG where USERNAME = '" + usernamePlayer2 + "' and GAMECODE = '" + splitData[0] + "'";
                DoSQLSelectCommand();

                if (_dataTable.Rows.Count == 0)
                {
                    _sql = "insert into BANGXEPHANG (USERNAME, GAMECODE, POINT) values (";
                    _sql += "'" + _username + "','" + usernamePlayer2 + "'," + 10 + ")";
                    _sqlCommand.CommandText = _sql;
                    _sqlCommand.ExecuteNonQuery();
                }
                else
                {
                    _sql = "update BANGXEPHANG set POINT =  POINT + " + 10 + " where USERNAME = '" + usernamePlayer2 + "' and GAMECODE = '" + splitData[0] + "'";
                    _sqlCommand.CommandText = _sql;
                    _sqlCommand.ExecuteNonQuery();
                }

                TcpClient tcpClient = (TcpClient)htClient[splitData[2]];
                StreamWriter sw = new StreamWriter(tcpClient.GetStream());

                string quitGameInfoMessage = "401";

                SendEncryptMessage(quitGameInfoMessage, sw);
            }

            public void GetRankList()
            {
                string getRankInfoMessage = "500";

                _sql = "select dense_rank() over (order by POINT desc) as [RANK], ACCOUNTNAME ,POINT ";
                _sql += "from BANGXEPHANG BXH join THONGTINTAIKHOAN TTTK on BXH.USERNAME = TTTK.USERNAME ";
                _sql += "where GAMECODE = 'OAQ'";
                DoSQLSelectCommand();

                if (_dataTable.Rows.Count != 0)
                {
                    for (int i = 0; i < _dataTable.Rows.Count; i++)
                    {
                        getRankInfoMessage += _dataTable.Rows[i].ItemArray[0] + "/**/" + _dataTable.Rows[i].ItemArray[1] + "/**/" + _dataTable.Rows[i].ItemArray[2] + "/**/";
                    }
                    _frmServerListen.InfoMessage(getRankInfoMessage + "\r\n");
                    SendEncryptMessage(getRankInfoMessage, _sw);
                }
                else
                {
                    _frmServerListen.InfoMessage(getRankInfoMessage + "\r\n");
                    SendEncryptMessage(getRankInfoMessage, _sw);
                }
            }

            public void SendResult()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);

                _sql = "select POINT from BANGXEPHANG where USERNAME = '" + _username + "' and GAMECODE = '" + splitData[0] + "'"; 
                DoSQLSelectCommand();

                int point = 0;

                if (splitData[1] == "win")
                {
                    point = 10;
                }
                else if (splitData[1] == "lose")
                {
                    point = -10;
                }

                if (_dataTable.Rows.Count == 0)
                {
                    if (point >= 0)
                    {
                        _sql = "insert into BANGXEPHANG (USERNAME, GAMECODE, POINT) values (";
                        _sql += "'" + _username + "','" + splitData[0] + "'," + point + ")";
                        _sqlCommand.CommandText = _sql;
                        _sqlCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        _sql = "insert into BANGXEPHANG (USERNAME, GAMECODE, POINT) values (";
                        _sql += "'" + _username + "','" + splitData[0] + "'," + 0 + ")";
                        _sqlCommand.CommandText = _sql;
                        _sqlCommand.ExecuteNonQuery();
                    }
                }
                else
                {
                    if ((int)_dataTable.Rows[0].ItemArray[0] >= 10)
                    {
                        _sql = "update BANGXEPHANG set POINT =  POINT + " + point + " where USERNAME = '" + _username + "' and GAMECODE = '" + splitData[0] + "'";
                        _sqlCommand.CommandText = _sql;
                        _sqlCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        if (point == 10)
                        {
                            _sql = "update BANGXEPHANG set POINT =  POINT + " + point + " where USERNAME = '" + _username + "' and GAMECODE = '" + splitData[0] + "'";
                            _sqlCommand.CommandText = _sql;
                            _sqlCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

            public void Ready()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);

                string getRoomInfoMessage = "310";
                ArrayList arrayList = (ArrayList)htGameRoom[splitData[0]];
                getRoomInfoMessage += splitData[0] + "/**/" + arrayList[0] + "/**/" + arrayList[1] + "/**/" + "true/**/";

                SendToAllPlayerInRoom(splitData[0], getRoomInfoMessage);
            }

            public void Unready()
            {
                _receiveData = _receiveData.Remove(0, 3);
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);

                string getRoomInfoMessage = "310";
                ArrayList arrayList = (ArrayList)htGameRoom[splitData[0]];
                getRoomInfoMessage += splitData[0] + "/**/" + arrayList[0] + "/**/" + arrayList[1] + "/**/" + "false/**/";

                SendToAllPlayerInRoom(splitData[0], getRoomInfoMessage);
            }

            public void Logout()
            {
                SendEncryptMessage("102", _sw);
                _login = false;
                UpdateStatus(_username, 0);
                DeleteClient(_client, _accountname);
            }

            public void UpdateStatus(string username, int status)
            {
                _sql = "update THONGTINTAIKHOAN set STATUS = " + status + " where USERNAME = '" + username + "'";
                SqlCommand cmd = new SqlCommand(_sql, _frmServerListen._sqlConnection);
                cmd.ExecuteNonQuery();
                if (status == 0)
                {
                    _frmServerListen.InfoMessage("User: " + username + " is offline!\r\n");
                }
                else if (status == 1)
                {
                    _frmServerListen.InfoMessage("User: " + username + " is online!\r\n");
                }
            }

            public void DoSQLSelectCommand()
            {
                _dataTable.Reset();
                if (_sqlDataReader != null)
                {
                    _sqlDataReader.Close();
                }
                _sqlCommand.CommandText = _sql;
                _sqlDataReader = _sqlCommand.ExecuteReader();
                _dataTable.Load(_sqlDataReader);
            }

            public int GetNumberPlayerInRoom(string room)
            {
                ArrayList temp = (ArrayList)htGameRoom[room];
                int numberPlayer = temp.Count;
                return numberPlayer;
            }

            public void GetRoomInfo()
            {
                _receiveData = _receiveData.Remove(0, 3);   
                string[] stringRemove = { "/**/" };
                string[] splitData = _receiveData.Split(stringRemove, StringSplitOptions.RemoveEmptyEntries);

                string getRoomInfoMessage = "310";
                ArrayList arrayList = (ArrayList)htGameRoom[splitData[0]];
                if (arrayList.Count == 1)
                {
                    getRoomInfoMessage += splitData[0] + "/**/" + arrayList[0] + "/**/" + "unknown" + "/**/" + "false/**/";
                }
                else if (arrayList.Count == 2)
                {
                    getRoomInfoMessage += splitData[0] + "/**/" + arrayList[0] + "/**/" + arrayList[1] + "/**/" + "false/**/";
                }

                _frmServerListen.InfoMessage(getRoomInfoMessage + "\r\n");
                SendEncryptMessage(getRoomInfoMessage, _sw);
            }

            public void SendEncryptMessage(string message, StreamWriter sw)
            {
                message = EncryptPass(message);
                sw.WriteLine(message);
                sw.Flush();
            }

            public void SendToAllPlayerInRoom(string room, string message)
            {
                ArrayList tempArray = (ArrayList)htGameRoom[room];
                foreach (var user in tempArray)
                {
                    TcpClient tcpClient = (TcpClient)htClient[user];
                    StreamWriter sw = new StreamWriter(tcpClient.GetStream());
                    SendEncryptMessage(message, sw);
                }
            }
        }
    }
}

