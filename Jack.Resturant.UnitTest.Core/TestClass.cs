using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Jack.Resturant.UnitTest.Core
{
    [TestClass]
    public class TestClass1
    {

        public static string PostJson(string url, object jsonObj, int timeout)
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(url);
                request.Method = "POST";
                request.ContinueTimeout = timeout;
                request.ContentType = "application/json; charset=utf-8";

                byte[] data = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj));
                request.ContentLength = data.Length;

                var task = request.GetRequestStreamAsync();
                task.Wait();
                var requestStream = task.Result;
                requestStream.Write(data, 0, data.Length);
                requestStream.Flush();

                var taskResponse = request.GetResponseAsync();
                taskResponse.Wait();
                var responseStream = taskResponse.Result.GetResponseStream();

                var contentType = taskResponse.Result.ContentType;//Content-Type: text/html; charset=GBK
                var match = System.Text.RegularExpressions.Regex.Match(contentType, @"charset\=([\w|\-]+)");
                var charsetCode = Encoding.UTF8;
                if (match != null && !string.IsNullOrEmpty(match.Value))
                {
                    string charset = match.Groups[1].Value;
                    try
                    {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        charsetCode = Encoding.GetEncoding(charset);
                    }
                    catch
                    {

                    }
                }

                StreamReader sr = new StreamReader(responseStream, charsetCode);
                var result = sr.ReadToEnd().Trim();
                responseStream.Dispose();
                requestStream.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Keccak256(string content)
        {
            var bs = HoshoEthUtil.Keccak.ComputeHash(new Span<byte>(System.Text.Encoding.UTF8.GetBytes(content)));
            var sb = new StringBuilder();
            foreach (var b in bs)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 把32个字节，转为uint
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public uint GetUint(byte[] bs)
        {
            uint result = 0;
            for(int i =  bs.Length  - 1; i >=0; i --)
            {
                result |= ((uint)bs[i]) << (bs.Length - 1 - i)*8;
            }
            return result;
        }
        /// <summary>
        /// 把16进制字符串，转为List<byte>
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public List<byte> GetBytes(string content)
        {
            List<byte> bsdata = new List<byte>();
            for (int i = 0; i < content.Length; i += 2)
            {
                bsdata.Add((byte)Convert.ToInt32(content.Substring(i, 2), 16));
            }
            return bsdata;
        }
        /// <summary>
        /// 获取结果，string类型的
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string GetResultForString(string str)
        {
            str = str.Substring(2);
            var data = GetBytes(str);
            //第一个32位，表示字符串内容放在什么位置
            var position = GetUint(data.GetRange(0, 32).ToArray());
            //从字符串位置开始，第一个32位，表示字符串长度
            var strLen = (int)GetUint(data.GetRange((int)position, 32).ToArray());
           
            var strData = data.GetRange((int)position + 32, strLen).ToArray();
            var strContent = System.Text.Encoding.UTF8.GetString(strData);
            return strContent;
        }
        /// <summary>
        /// 获取结果,uint[]类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public uint[] GetResultForUintArray(string str)
        {
            str = str.Substring(2);
            var data = GetBytes(str);
            uint[] result = new uint[data.Count / 32];
            for(int i = 0; i < result.Length; i ++)
            {
                result[i] = GetUint( data.GetRange(i * 32, 32).ToArray());
            }
            return result;
        }
        public string GetParametersString(params object[] parameters)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach ( var p in parameters )
            {
                if(p is int)
                {
                    //uint256
                    byte[] bs = BitConverter.GetBytes((int)p);
                    for(int i = 0; i < 32 - bs.Length; i ++)
                    {
                        strBuilder.Append("00");
                    }
                    for(int i = bs.Length - 1; i >= 0; i --)
                    {
                        strBuilder.Append(bs[i].ToString("x2"));
                    }
                }
            }
            return strBuilder.ToString();
        }

        [TestMethod]
        public void test()
        {
            var objs = GetResultForUintArray("0x00000000000000000000000000000000000000000000000000000000000001640000000000000000000000000000000000000000000000000000000000000164");
            var funcHex = Keccak256("say2(uint256)").Substring(0,8);

            /*
               {"jsonrpc":"2.0","method": "eth_getCompilers", "id": 3}
               */

            var dict = new Dictionary<string, object>() {
                { "id" , 1},
                 { "jsonrpc" , "2.0"},
                  { "method" , "personal_unlockAccount"},
                  { "params" , new object[]{ "0x4e82a8a91ceec8a9e471c0b6808304e1914c6be7", "TanYong" } }
            };
            var result = PostJson("http://192.168.136.141:8545/rpc", dict, 8000);
            var parameterStr = GetParametersString(3);
            dict = new Dictionary<string, object>() {
                { "id" , 8},
                 { "jsonrpc" , "2.0"},
                  { "method" , "eth_sendTransaction"},
                  { "params" , new object[]{ new { from= "0x4e82a8a91ceec8a9e471c0b6808304e1914c6be7", to = "0x24d25a91a95a7b443462e1c5f2cc743ccaf89b0c", data = $"0x{funcHex}{parameterStr}" } } }
            };
            result = PostJson("http://192.168.136.141:8545/rpc", dict, 8000);
            //0x9273e4b3b01d74415cb5c585d24537de87469a3d5ed868071d7c14002c3db77b

            var tranHex = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result)["result"];
            //

            dict = new Dictionary<string, object>() {
                { "id" , 8},
                 { "jsonrpc" , "2.0"},
                  { "method" , "eth_getTransactionReceipt"},
                  { "params" , new object[]{ tranHex } }
            };
            result = PostJson("http://192.168.136.141:8545/rpc", dict, 8000);
        }

        [TestMethod]
        public void test2()
        {

            var dict = new Dictionary<string, object>() {
                { "id" , 3},
                 { "jsonrpc" , "2.0"},
                  { "method" , "eth_getTransactionReceipt"},
                  { "params" , new object[]{ "0xff4bb125b6e217aad36c01ee4d702d10a9dde74366703de1425762ef81a69627" } }
            };
            var result = PostJson("http://192.168.136.137:8545", dict, 8000);
            //0x9273e4b3b01d74415cb5c585d24537de87469a3d5ed868071d7c14002c3db77b
        }
    }
}
