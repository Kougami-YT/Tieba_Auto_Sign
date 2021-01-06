using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace Tieba_Auto_Sign
{
    class Program
    {
        static string url_index = "https://tieba.baidu.com/index.html";
        static string url_sign = "https://tieba.baidu.com/sign/add";
        public static string cookie;
        static Dictionary<string, string> header = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            Console.WriteLine("请输入贴吧Cookie：");
            cookie = Console.ReadLine();
            List<Tieba> Info_Tieba = GetTiebaInfo(cookie);
            Sign(Info_Tieba);
        }
        public static List<Tieba> GetTiebaInfo(string cookie) //使用网上POST方法
        {
            header.Clear();
            header.Add("Cookie", cookie);
            string result_html = PostMoths(url_index, null, header);
            string result = Mid(result_html, "\"forums\":", "]") + "]";
            return JsonConvert.DeserializeObject<List<Tieba>>(result);
        }
        public static void Sign(List<Tieba> info) //使用RestSharp
        {
            RestClient client = new RestClient(url_sign);
            client.Timeout = -1;
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", cookie);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("ie", "utf-8");

            foreach (Tieba i in info)
            {
                request.AddOrUpdateParameter("kw", i.forum_name);
                IRestResponse response = client.Execute(request);

                Result_Sign result = JsonConvert.DeserializeObject<Result_Sign>(response.Content);


                Console.WriteLine("————————————————————");
                if (result.no == 0)
                {
                    Console.WriteLine(i.forum_name + "吧签到成功！");
                }
                else if (result.no == 1101)
                {
                    Console.WriteLine(i.forum_name + "吧已签到");
                    Console.WriteLine("等级：" + i.level_id);
                    Console.WriteLine("等级名称：" + i.level_name);
                }
                else
                {
                    Console.WriteLine(i.forum_name + "吧签到失败");
                    Console.WriteLine("错误代码：" + result.no);
                    Console.WriteLine("错误提示：" + result.error);
                }
            }
            Console.WriteLine("————————————————————");
            Console.WriteLine("执行完毕！总数：" + info.Count);
        }
        public static string PostMoths(string url, object obj_model, Dictionary<string, string> dic = null)
        {
            string param = JsonConvert.SerializeObject(obj_model);
            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            if (dic != null && dic.Count != 0)
            {
                foreach (var item in dic)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }
            byte[] payload;
            payload = System.Text.Encoding.UTF8.GetBytes(param);
            request.ContentLength = payload.Length;
            string strValue = "";
            try
            {
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();
                System.Net.HttpWebResponse response;
                response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream s;
                s = response.GetResponseStream();
                string StrDate = "";
                StreamReader Reader = new StreamReader(s, Encoding.UTF8);
                while ((StrDate = Reader.ReadLine()) != null)
                {
                    strValue += StrDate;
                }
            }
            catch (Exception e)
            {
                strValue = e.Message;
            }
            return strValue;
        } //发送POST请求 带请求头、请求体
        public static string Mid(string source, string left, string right, int position = 0)
        {
            int start, end;
            string result;
            source = source.Substring(position, source.Length - position);
            start = source.IndexOf(left) + left.Length;
            if (start == -1 + left.Length)
            {
                return "*nothing*";
            }
            source = source.Substring(start, source.Length - start);
            end = source.IndexOf(right);
            if (end == -1)
            {
                return "*nothing*";
            }
            result = source.Substring(0, end);
            return result;
        } //取两端字符串中间的字符串
        public static T XmlToClass<T>(string xml) where T : class
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(sr) as T;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        } //json文本转实体类
    }
}

public class Body
{
    public string ie { get; set; }
    public string kw { get; set; }
    public Body(string ie, string kw)
    {
        this.ie = ie;
        this.kw = kw;
    }
}
public class Tieba
{
    public int user_id { get; set; }
    public int forum_id { get; set; }
    public string forum_name { get; set; }
    public int is_like { get; set; }
    public int is_black { get; set; }
    public int like_num { get; set; }
    public int is_top { get; set; }
    public int in_time { get; set; }
    public int level_id { get; set; }
    public string level_name { get; set; }
    public int cur_score { get; set; }
    public int score_left { get; set; }
    public int sort_value { get; set; }
    public int is_sign { get; set; }
}

public class Result_Sign
{
    public int no { get; set; }
    public string error { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public int errno { get; set; }
    public string errmsg { get; set; }
    public int sign_version { get; set; }
    public int is_block { get; set; }
    public Finfo finfo { get; set; }
    public Uinfo uinfo { get; set; }
}

public class Finfo
{
    public Forum_Info forum_info { get; set; }
    public Current_Rank_Info current_rank_info { get; set; }
}

public class Forum_Info
{
    public int forum_id { get; set; }
    public string forum_name { get; set; }
}

public class Current_Rank_Info
{
    public int sign_count { get; set; }
}

public class Uinfo
{
    public int user_id { get; set; }
    public int is_sign_in { get; set; }
    public int user_sign_rank { get; set; }
    public int sign_time { get; set; }
    public int cont_sign_num { get; set; }
    public int total_sign_num { get; set; }
    public int cout_total_sing_num { get; set; }
    public int hun_sign_num { get; set; }
    public int total_resign_num { get; set; }
    public int is_org_name { get; set; }
}

