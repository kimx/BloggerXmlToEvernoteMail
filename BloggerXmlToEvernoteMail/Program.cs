using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BloggerXmlToEvernoteMail
{
    /// <summary>
    /// 將Blogger匯出來的xml配合Evernote mail format 匯入
    /// https://help.evernote.com/hc/zh-tw/articles/209005347-%E5%A6%82%E4%BD%95%E5%84%B2%E5%AD%98%E9%9B%BB%E5%AD%90%E9%83%B5%E4%BB%B6%E5%88%B0-Evernote
    /// 注意事項:
    /// 1.匯入的筆記本及Tag要先建立好才會Mapping起來
    /// 2.新建立的tag最好在筆記本內先用一個空白note先tag起來，否則匯入時會找不到.
    /// </summary>
    class Program
    {
        static string _SmtpHost = "smtp.gmail.com";
        static int _SmtpPort = 587;
        static bool _SmtpEnabledSsl = true;
        static string _SmtpAccount = "xxx@gmail.com";
        static string _SmtpPassword = "xxx";

        static string _InputPath = @"E:\blog.xml";//要匯入的xml檔案
        static string _EvernoteNote = "Blog-Temp";//要匯入在哪一本Evernote的名稱
        static string _EvernoteMail = "xxxx@gmail.com";//Evernote接收的Mail account
        static void Main(string[] args)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(new FileStream(_InputPath, FileMode.Open));
            var entryNodes = doc.DocumentElement.GetElementsByTagName("entry");
            var list = GetImportData(entryNodes);
            SendMailToEvernote(list);
            Console.WriteLine("End");
            Console.ReadLine();
        }

        static List<ImportModel> GetImportData(XmlNodeList nodeList)
        {
            List<ImportModel> list = new List<ImportModel>();
            foreach (XmlNode node in nodeList)
            {
                if (node.InnerXml.IndexOf("term=\"http://schemas.google.com/blogger/2008/kind#post\"") < 0)//只找文章，忽略留言及設定檔
                    continue;
                ImportModel item = new ImportModel();
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "title")//文章標題
                        item.Title = child.InnerText;
                    else if (child.Name == "content")//文章內容
                        item.Content = child.InnerText;
                    else if (child.Name == "published")
                        item.Tags.Add($"#{child.InnerText.Substring(0, 4)}");//取發佈年份作tag
                    else if (child.Name == "category" && child.Attributes["scheme"] != null && child.Attributes["scheme"].Value.StartsWith("http://www.blogger.com/atom/"))
                        item.Tags.Add($"#{child.Attributes["term"].Value}");//取得tag
                }

                if (!string.IsNullOrEmpty(item.Title) && !string.IsNullOrEmpty(item.Content))
                    list.Add(item);
            }
            return list;
        }

        static void SendMailToEvernote(List<ImportModel> list)
        {
            SmtpClient client = new SmtpClient(_SmtpHost, _SmtpPort);
            client.Credentials = new NetworkCredential(_SmtpAccount, _SmtpPassword);
            client.EnableSsl = _SmtpEnabledSsl;
            int i = 0;
            foreach (ImportModel item in list)
            {
                string subject = $"{item.Title} @{_EvernoteNote} {string.Join(" ", item.Tags.ToArray())}";//格式化主旨:標題 筆記本名稱 tags
                MailMessage mail = new MailMessage(_SmtpAccount, _EvernoteMail);
                mail.IsBodyHtml = true;
                mail.Body = item.Content;
                mail.Subject = subject;
                try
                {
                    i++;
                    Console.WriteLine(i + "  : " + subject);
                    client.Send(mail);
                    System.Threading.Thread.Sleep(7000);//每一筆傳送後,稍微停一下,才不會被Evernote認為異常而發行錯誤. 
                }
                catch
                {
                    Console.WriteLine("Error Index:" + i);
                }
            }
        }
    }

    class ImportModel
    {
        public ImportModel()
        {
            this.Tags = new List<string>();
        }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> Tags { get; set; }
    }

}
