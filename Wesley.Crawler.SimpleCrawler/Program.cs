using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Net;
using Wesley.Crawler.SimpleCrawler.Models;

using System.Data;
using System.Data.SqlClient;

namespace Wesley.Crawler.SimpleCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            CollegeCrawler();
            Console.ReadKey();
        }

        public static void CollegeCrawler()
        {
            using (SqlConnection con = new SqlConnection())
            {
                int i = 1;
                //连接数据库
                //con.ConnectionString = @"data source=*****;initial catalog=YGDatabase;persist security info=True;user id=sa;password=******;MultipleActiveResultSets=True;App=EntityFramework&quot;";
                //con.Open();

                
                var cityUrl = "http://college.gaokao.com/schlist/p{0}/";//定义爬虫入口URL
                var collegeList = new List<College>();//
                var cityCrawler = new SimpleCrawler();//调用刚才写的爬虫程序

                cityCrawler.OnStart += (s, e) =>
                {
                    Console.WriteLine("开始-地址：" + e.Uri.ToString());
                };
                cityCrawler.OnError += (s, e) =>
                {
                    Console.WriteLine("错误：" + e.Uri.ToString() + "，异常消息：" + e.Exception.Message);

                    //using (FileStream fs = new FileStream(@"C:\Users\Administrator\Desktop\college.txt", FileMode.Create, FileAccess.Write))
                    //{
                    //    StreamWriter sw = new StreamWriter(fs);
                    //    sw.WriteLine("URL:"+e.Uri);//开始写入值
                    //    sw.Close();
                    //}
                };
                cityCrawler.OnCompleted += (s, e) =>
                {
                    //使用正则表达式清洗网页源代码中的数据
                    var links = Regex.Matches(e.PageSource, @"<img[^>]+src=""(?<img>.*?)"" onerror=(.*?)alt=""(?<title>.*?)"" />", RegexOptions.IgnoreCase);
                    foreach (Match match in links)
                    {
                        var college = new College
                        {
                            title = match.Groups["title"].Value,
                            img_src = match.Groups["img"].Value
                        };
                        if (!collegeList.Contains(college)) collegeList.Add(college);//将数据加入到泛型列表
                        WebClient MyWebClient = new WebClient();



                        //SqlCommand com = new SqlCommand();

                        //com.Connection = con;
                        //com.CommandType = CommandType.Text;
                        //com.CommandText = string.Format("select TOP 1 ID from [LiePinDataBase].[dbo].[sys_college_info]where name='{0}'", college.title);
                        //object dr1 = com.ExecuteScalar();//执行SQL语句

                        //if (dr1==null)
                        //{


                            var icon = Guid.NewGuid().ToString();

                            var name = college.title;

                            string src = @"C:\Users\Administrator\Desktop\images\" + icon + @".jpg";
                            MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                            MyWebClient.Headers["User-Agent"] = "blah";

                            Byte[] jpgdata = new Byte[] { };

                            if (match.Groups["img"].Value != @"http://college.gaokao.com/style/college/images/icon_default.png")
                            {
                                try
                                {
                                    jpgdata = MyWebClient.DownloadData(college.img_src); //从指定网页下载数据;

                                    using (FileStream fs = new FileStream(src, FileMode.OpenOrCreate, FileAccess.Write))
                                    {
                                        fs.Write(jpgdata, 0, jpgdata.Length);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    icon = "icon_default";
                                    Console.WriteLine("URL:" + e.Uri + "大学:" + match.Groups["title"].Value + "找不到图片");
                                }
                            } else
                            {
                                icon = "icon_default";
                            }


                            //com.Connection = con;
                            //com.CommandType = CommandType.Text;
                            //com.CommandText = string.Format("insert into [LiePinDataBase].[dbo].[sys_college_info](name,icon) values ('{0}','{1}')", name, icon);
                            //SqlDataReader dr = com.ExecuteReader();//执行SQL语句
                            //dr.Close();//关闭执行

                            Console.WriteLine(college.title + "|" + college.img_src);
                        }
                       
                        
                    //}
                    Console.WriteLine("===============================================");
                    Console.WriteLine("完成抓抓取了 " + links.Count + " 个大学。");

                    if (i <= 107)
                    {
                        i++;
                        var url = string.Format(cityUrl, i);
                        cityCrawler.Start(new Uri(url), null, "gb2312").Wait();
                    }
                };

                cityCrawler.Start(new Uri("http://college.gaokao.com/schlist/p1/"), null, "gb2312").Wait();
            }
        }

    }
}




