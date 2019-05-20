using Microsoft.VisualStudio.TestTools.UnitTesting;
using fname2timestamp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace fname2timestamp.Tests
{
    [TestClass()]
    public class FileListModelTests
    {
        public static FileListModel fmodel;
        public class ExpActFileNameList
        {
            public string _fname;
            public DateTime _expdate;

        }

        [ClassInitialize]
        public static void FileListModelClassInitialize(TestContext testContext)
        {
            fmodel = new FileListModel();
        }
        
        /// <summary>
        /// 日付データが含まれない場合の例外
        /// </summary>
        [TestMethod()]
        public void GetDataTime_NotDate()
        {
            var flist = new Dictionary<string, DateTime>
            {
                { "a.txt",new DateTime()},
                { "99b99e9394r99.txt",new DateTime()},
                { "dd0dfdsaf.txt",new DateTime()},
                { "長い文字列パターン.txt",new DateTime()},
                { "34234d.txt",new DateTime()},
                { "12378-23131d1.txt",new DateTime()},
                { "2034.txt",new DateTime()},
                { "２０２０１１２５０３１２２２.txt",new DateTime()},
            };
            var pbobj = new PrivateObject(fmodel);
            foreach (var data in flist)
            {
                try
                {

                    DateTime datelist = (DateTime)pbobj.Invoke("GetDataTime", data.Key);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException == null) continue;

                    if (!(ex.InnerException.GetType() == typeof(ArgumentException)) && !(ex.InnerException is System.FormatException))
                    {
                        Assert.Fail();
                    }
                }
            }

        }
        /// <summary>
        /// 日付として異常な変換不能例外のテスト
        /// </summary>
        [TestMethod()]
        public void GetDataTime_IllegalSixDate()
        {

            var flist = new Dictionary<string, DateTime>
            {
                { "20191111-232260.txt",new DateTime()},
                { "29191111-232211.txt",new DateTime()},
                { "20191311-232211.txt",new DateTime()},
                { "20191111-242211.txt",new DateTime()},
                { "20190228-232211.txt",new DateTime()},
            };
            var pbobj = new PrivateObject(fmodel);
            foreach (var data in flist)
            {
                try { 
                    DateTime datelist = (DateTime)pbobj.Invoke("GetDataTime", data.Key);
                }
                catch (TargetInvocationException ex)
                { 
                    if (ex.InnerException == null) continue;
                    if (!(ex.InnerException.GetType() == typeof(ArgumentOutOfRangeException)))
                    {
                        Assert.Fail();
                    }
                }
            }
        }
        /// <summary>
        /// 正常な日付データ
        /// </summary>
        [TestMethod()]
        public void GetDataTime_ValidSixData()
        {
            var flist = new Dictionary<string,DateTime>
            {
                { "20191111-232201.txt",new DateTime(2019,11,11,23,22,1)},
                { "21191111-232211.txt",new DateTime(2119,11,11,23,22,11)},
                { "20191211-232201.txt",new DateTime(2019,12,11,23,22,1)},
                { "20191111-212211.txt",new DateTime(2019,11,11,21,22,11)},
                { "20190101-000000.txt",new DateTime(2019,1,1,0,0,0)},
            };
            var pbobj = new PrivateObject(fmodel);
            foreach (var data in flist)
            {
                DateTime datelist = (DateTime)pbobj.Invoke("GetDataTime", data.Key);
                Assert.AreEqual(datelist, data.Value);
            }
        }
        /// <summary>
        /// 正常な日付データ（秒データだけなし）
        /// </summary>
        [TestMethod()]
        public void GetDataTime_ValidFiveDate()
        {
            var flist = new Dictionary<string, DateTime>
            {
                { "20191111-2322.txt",new DateTime(2019,11,11,23,22,0)},
                { "29191111-2322fasdfa.txt",new DateTime(2919,11,11,23,22,0)},
                { "20190111-2322ab.txt",new DateTime(2019,1,11,23,22,0)},
                { "20191101-2122dd.txt",new DateTime(2019,11,1,21,22,0)},
                { "20191231-2322うわうわ",new DateTime(2019,12,31,23,22,0)},
            };
            var pbobj = new PrivateObject(fmodel);
            foreach (var data in flist)
            {
                DateTime datelist = (DateTime)pbobj.Invoke("GetDataTime", data.Key);
                Assert.AreEqual(datelist,data.Value);
            }
        }
        /// <summary>
        /// 正常な日付データ（秒データだけなし）
        /// </summary>
        [TestMethod()]
        public void GetDataTime_ValidDateOnly()
        {
            var flist = new Dictionary<string, DateTime>
            {
                { "20191111fadfa.txt",new DateTime(2019,11,11,0,0,0)},
                { "だｆだｓｄふぁ29191111.txt",new DateTime(2919,11,11,0,0,0)},
                { "20190111.txt",new DateTime(2019,1,11,0,0,0)},
                { "abc20191101.txt",new DateTime(2019,11,1,0,0,0)},
                { "abc20191231eee",new DateTime(2019,12,31,0,0,0)},
            };
            var pbobj = new PrivateObject(fmodel);
            foreach (var data in flist)
            {
                DateTime datelist = (DateTime)pbobj.Invoke("GetDataTime", data.Key);
                Assert.AreEqual(datelist, data.Value);
            }
        }
        /// <summary>
        /// 記号によるセパレータの正常な日付データ
        /// </summary>
        [TestMethod()]
        public void GetDataTime_AmbiguousSixData()
        {
            var flist = new Dictionary<string, DateTime>
            {
                { "2019-1-11:02-22-01.txt",new DateTime(2019,1,11,2,22,1)},
                { "2019a1a11b02b22b1d.txt",new DateTime(2019,1,11,2,22,1)},
                { "2019あ1あ11い02え22う01ば.txt",new DateTime(2019,1,11,2,22,1)},
                { "2019!1!11!02!22!01!!!1123123.txt",new DateTime(2019,1,11,2,22,1)},
                { "2019年1月11日2時22分1秒.txt",new DateTime(2019,1,11,2,22,1)},
            };
            var pbobj = new PrivateObject(fmodel);
            foreach (var data in flist)
            {
                DateTime datelist = (DateTime)pbobj.Invoke("GetDataTime", data.Key);
                Assert.AreEqual(datelist, data.Value);
            }
        }
        [TestMethod()]
        public void GetDataTime_PrefixAndSuffixSixData()
        {
            var flist = new Dictionary<string, DateTime>
            {
                { "ええふぁ20191111-232201ｆだｆ.txt",new DateTime(2019,11,11,23,22,1)},
                { "うとつと21191111-232211fsdfdfa.txt",new DateTime(2119,11,11,23,22,11)},
                { "うごうご20191211-232201wwwww.txt",new DateTime(2019,12,11,23,22,1)},
                { "あんびえんと」20191111-212211wefawef.txt",new DateTime(2019,11,11,21,22,11)},
            };
            var pbobj = new PrivateObject(fmodel);
            foreach (var data in flist)
            {
                DateTime datelist = (DateTime)pbobj.Invoke("GetDataTime", data.Key);
                Assert.AreEqual(datelist, data.Value);
            }
        }

    }
}