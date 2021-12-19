
using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace Program
{
    internal class Program
    {
		
		public static void ProcessClientRequests(object argument)
		{
			TcpClient client = (TcpClient)argument;// tcp client
			try
			{
				StreamReader reader = new StreamReader(client.GetStream());// read data
				StreamWriter writer = new StreamWriter(client.GetStream());// send data
				String session = "";// lưu trạng thái đăng nhập
				Console.WriteLine("Client ket noi thanh cong " + client.Client.RemoteEndPoint.ToString());
				while (true)// còn đang kết nối
				{
					string s = reader.ReadLine();		// nhận data gởi từ client			
					Console.WriteLine("Hanh dong: " + s);
					if(s.Equals("login"))// login
                    {
				
					int checkUser = 0;// ktra user 0 : sai | 1 : đúng
					int checkPass = 0;// kra pass 0 : sai | 1 : đúng
					string u = reader.ReadLine(); // nhận user từ client gỏi
					Console.WriteLine("username -> " + u);
					string p= reader.ReadLine();// nhận pass
					Console.WriteLine("password -> " + p);

                    string[] a = File.ReadAllLines("user.txt");// đọc từ file lưu data user.txt
                        for (int i = 0; i < a.Length; ++i) //duyeẹt qua các dòng
                        {
                            string[] file = a[i].Split('/');// tách chuỗi bởi dấu "/" user/pass 
                            if (u.Equals(file[0]))// file[0] :user
                            {
                                checkUser = 1;// user true
                            }
                            if (p.Equals(file[1])) //file[1]" pass
                            {
                                checkPass = 1;// pass true
                            }

                        }
                        if (session.Equals(u))// user sai
						{
							writer.WriteLine("Tai khoan da dang nhap"); // gởi thông báo qua client
							writer.Flush();
							Console.WriteLine("Tai khoan da dang nhap");
						}
						else
						{
							if (checkUser == 1)
							{
								if (checkPass == 1) //login thanh công
								{
									writer.WriteLine("Dang nhap thanh cong");
									writer.Flush();
									Console.WriteLine("Dang nhap thanh cong");
									session = u;// luu tên nguoi login , ( xét nếu đã login thì ko cần login nữa)
								}
								else// sai pass
								{
									writer.WriteLine("Sai mat khau");
									writer.Flush();
									Console.WriteLine("Sai mat khau");
								}
							}
							else// user false
							{
								writer.WriteLine("Tai khoan chua ton tai");
								writer.Flush();
								Console.WriteLine("Tai Khoan chua ton tai");
							}
						}
					
					}	
					if(s.Equals("register"))// register
                    {
						int checkRegister = 0; // ktre user đã đăng kí chưa
						string[] a = File.ReadAllLines("user.txt"); // đọc file
						string u = reader.ReadLine();
						string p = reader.ReadLine();
						for (int i = 0; i < a.Length; ++i)
						{
							string[] file = a[i].Split('/');// tách chuỗi "/"
							if (u.Equals(file[0]))
							{
								checkRegister = 1; // đã dki
							}

						}
						if (checkRegister == 0) //chưa đki
						{
							string b = u + "/" + p; // user/pass
							File.AppendAllText("user.txt", b + Environment.NewLine); // lưu xuống file ( có xuống dòng)
							// xem cấu trúc lưu trong file user.txt
							writer.WriteLine("Dang ki thanh cong");// send status client
							writer.Flush();
							Console.WriteLine("Dang ki thanh cong");
							
						}
						if (checkRegister == 1)// đã đki
						{
							writer.WriteLine("Tai khoan da ton tai");
							writer.Flush();
							Console.WriteLine("Tai khoan da ton tai");
						}
					}
					if(s.Equals("search")) // tìm kiếm
                    {
						string key = reader.ReadLine();// đọc từ khóa client gởi
						Console.WriteLine("tim kiem voi tu khoa: "+ key );
                        XmlDocument xml = new XmlDocument();
						// Lấy dữ liệu dạng XML từ api 
						// nhấn vào link để xem cấu trúc data xml
                        xml.Load("http://www.vietcombank.com.vn/exchangerates/ExrateXML.aspx");
                        XmlNodeList noXml;
						// Rút trích dữ liệu
                        noXml = xml.SelectNodes("/ExrateList/Exrate");
                        int i = 0;
						string result = null;
                        for (i = 0; i <= noXml.Count - 1; i++) // duyệt data
                        {
                            if (noXml.Item(i).Attributes["CurrencyCode"].InnerText.Equals(key))// tìm xem loại tiền tệ
																					// //client gởi có trong data api hay ko
                            {
                                string CurrencyCode = noXml.Item(i).Attributes["CurrencyCode"].InnerText;// lấy dữ liệu currencycode
                                string Currencyname = noXml.Item(i).Attributes["CurrencyName"].InnerText;
                                string Buy = noXml.Item(i).Attributes["Buy"].InnerText;
                                string Transfer = noXml.Item(i).Attributes["Transfer"].InnerText;
                                string Sell = noXml.Item(i).Attributes["Sell"].InnerText;
								result =  CurrencyCode + "/" + Currencyname + "/" + Buy + "/" + Transfer + "/" + Sell;
								//cộng chuỗi để gởi client  cách nhau dấu "/" ( gởi dạng chuỗi cho dễ )
                            }
                        }
						writer.WriteLine(result);// gởi kq tìm kiếm cho client
						writer.Flush();
						Console.WriteLine("Du lieu tim kiem: "+result);

					}
					if (s.Equals("Exit")){ // hủy knoi

						Console.WriteLine("Client "+ client.Client.RemoteEndPoint.ToString()+ " huy ket noi !!");
						writer.Close();
						reader.Close();
						client.Close();
						break;

					}
				}
            }
			catch (IOException)
			{
				Console.WriteLine("Client ket noi khong thanh cong - mat ket noi !!! "  +client.Client.RemoteEndPoint.ToString());
			}
        }

		


		public static void Main()
		{
			TcpListener listener = null;
			try
			{
				listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);// Server lắng nghe Client có Ip,port phù hợp
				listener.Start();
				Console.WriteLine("Server khoi dong thanh cong (127.0.0.1/5000)");
				Console.WriteLine("Dang cho client ket noi...");
				while (true)// có client kết nối
				{
					TcpClient client = listener.AcceptTcpClient();// chấp nhập kết nối client
					Thread t = new Thread(ProcessClientRequests);// phân luồng thread để quản lí nhìu client
					// nhìu client connect 1 server
					t.Start(client);//
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		} 
	}
}