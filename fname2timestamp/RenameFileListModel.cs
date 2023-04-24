using DataGridFiles;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static fname2timestamp.FileListModel;

namespace fname2timestamp
{

    /// <summary>
    /// ファイルリスト管理クラス
    /// </summary>
    public class RenameFileListModel : BindableBase
    {
        private ObservableCollection<DataGridFile> dataGridFiles;
        public ObservableCollection<DataGridFile> DataGridFiles
        {
            get { return dataGridFiles; }
            set { this.SetProperty(ref dataGridFiles, value); }
        }


        /// <summary>
        /// リスト内の全ファイル数
        /// </summary>
        private int fileListCount;
        public int FileListCount
        {
            get => fileListCount;
            set => SetProperty(ref fileListCount, value);
        }
        /// <summary>
        /// 変換可能なファイル数
        /// </summary>
        private int canFileListCount;
        public int CanFileListCount
        {
            get => canFileListCount;
            set => SetProperty(ref canFileListCount, value);
        }

        /// <summary>
        /// 進捗関連
        /// </summary>
        private int currentProgress;
        public int CurrentProgress
        {
            get => currentProgress;
            set => SetProperty(ref currentProgress, value);
        }
        public RenameFileListModel()
        {
            //dataGridFiles = new ObservableCollection<DataGridFile>();
            DataGridFiles = new ObservableCollection<DataGridFile>();//DataGridにbindingしているデータクラス
            DataGridFiles.CollectionChanged += DataGridFiles_CollectionChanged;
        }

        private void DataGridFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FileListCount = dataGridFiles.Count;
            CanFileListCount = dataGridFiles.Count(x => x.isValid == true);
        }

        /// <summary>
        /// ドロップされたフォルダ及びファイルからファイルリストを作成する
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>        
        private List<string> CreateDropFileList(IEnumerable<string> files)
        {
            List<string> allfiles = new List<string>();

            if (files != null)
            {
                foreach (string f in files)
                {
                    //フォルダの場合
                    if (System.IO.Directory.Exists(f))
                    {
                        //サブフォルダ含めて全ファイルリストを取得
                        IEnumerable<string> d_under_files = System.IO.Directory.EnumerateFiles(f, "*", System.IO.SearchOption.AllDirectories);
                        foreach (string duf in d_under_files)
                        {
                            if (System.IO.File.Exists(duf))
                            {
                                allfiles.Add(duf);
                            }
                        }
                    }
                    //ファイルの場合
                    else if (System.IO.File.Exists(f))
                    {
                        allfiles.Add(f);

                    }
                }
            }
            return allfiles;
        }
        /// <summary>
        /// ファイルリストからデータグリッドクラスにデータを格納する
        /// </summary>
        /// <param name="files"></param>
        /// <param name="upflag"></param>
        public int DropFileListToDataGridFile(IEnumerable<string> files, UPDATE_FLAG upflag)
        {
            int fcount = 0;
            CurrentProgress = 0;

            List<string> allfiles = CreateDropFileList(files);

            foreach (string f in allfiles)
            {
                CurrentProgress = ((fcount++ * 100) / allfiles.Count);
                var a = ConvertToDataGridFile(f, upflag);
                DataGridFiles.Add(a);
            }
            CurrentProgress = 0;
            return DataGridFiles.Count;

        }
        /// <summary>
        /// ファイル名から時刻情報を配列に取り出す
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        private bool GetDataTimeFromFileName(string fname,out string matchString,out DateTime dateTime)
        {
            dateTime = new DateTime();
            matchString = "";
            //YYYY-MM-DD-mm-dd-ss
            Regex dtimePattern_spl = new Regex(@"(\d{4})[\D]+(\d{2})[\D]+(\d{2})[\D]+(\d{2})[\D]+(\d{2})(?:[\D]+(\d+)){0,1}", System.Text.RegularExpressions.RegexOptions.Compiled);

            //YYYYMMDD-mmddss
            Regex dtimePattern_seq = new Regex(@"(\d{4})(\d{2})(\d{2})[\D]{0,1}(\d{0,2})(\d{0,2})(\d{0,2})", System.Text.RegularExpressions.RegexOptions.Compiled);
            List<int> dtl = new List<int>();
            Match m_spl = dtimePattern_spl.Match(fname);
            Match m_seq = dtimePattern_seq.Match(fname);
            if (m_spl.Success || m_seq.Success)
            {
                Match m = m_spl.Success ? m_spl : m_seq;
                matchString = m.Value;
                for (int i = 1; i < m.Groups.Count; i++)
                {
                    for (int j = 0; j < m.Groups[i].Captures.Count; j++)
                    {
                        if (m.Groups[i].Captures[j].Value != "")
                        {
                            dtl.Add(int.Parse(m.Groups[i].Captures[j].Value));
                        }
                    }
                }
                if (dtl.Count >= 3)//少なくとも年月日は必要
                {
                    for (int i = dtl.Count; i < 6; i++)//時分秒がない場合の補完
                    {
                        dtl.Add(0);
                    }
                }
            }
            if(dtl.Count() != 6)
            {
                return false;
            }
            dateTime = new DateTime(dtl[0], dtl[1], dtl[2], dtl[3], dtl[4], dtl[5]);
            return true;
        }
        /// <summary>
        /// ファイルパスからデータグリッド表示用のクラスに変換する
        /// </summary>
        /// <param name="fpath"></param>
        /// <param name="upflag"></param>
        /// <returns></returns>
        private DataGridFile ConvertToDataGridFile(string fpath, UPDATE_FLAG upflag)
        {
            string errmsg = "";
            bool isValid = false;
            string fname = "";
            string fext = "";
            long fsize = 0;
            string matchString = "";
            System.IO.FileAttributes attr;
            DateTime renDt = new DateTime();
            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(fpath);
                fsize = fi.Length;
                attr = System.IO.File.GetAttributes(fpath);
                fname = System.IO.Path.GetFileName(fpath);
                fext = System.IO.Path.GetExtension(fpath);
                //読み取り専用属性があるか調べる
                if ((attr & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
                {
                    throw new System.UnauthorizedAccessException("ファイルが読み取り専用属性のためリネームできません");
                }

                if(true == GetDataTimeFromFileName(fname,out matchString,out DateTime dt) )
                { 
                    throw new ArgumentException("ファイル名に日付情報がすでに入っています");
                }
                isValid = true;
                if (upflag.HasFlag(UPDATE_FLAG.CREATTION_DATE))
                {
                    renDt = System.IO.File.GetCreationTime(fpath);
                }
                else
                {
                    renDt = System.IO.File.GetLastWriteTime(fpath);
                }
            }
            catch (Exception exception)
            {
                errmsg = exception.Message;
            }
            DataGridFile dtf = new DataGridFile
            {
                //num = ++index,
                isValid = isValid,
                path = fpath,
                FileName = fname,
                update_dtime = System.IO.File.GetLastWriteTime(fpath),
                create_dtime = System.IO.File.GetCreationTime(fpath),
                access_dtime = System.IO.File.GetLastAccessTime(fpath),
                //f2t_dtime = dt,
                success = false,
                err_message = errmsg,
                 RenameFileName = isValid ? AfterFileName(fname, renDt) : "",
            };
            return dtf;
        }
        /// <summary>
        /// リストをオプションに従いリロードする
        /// </summary>
        /// <param name="upflag"></param>
        public void UpdateList(FileListModel.UPDATE_FLAG upflag)
        {
            for (int i = 0; i < dataGridFiles.Count; i++)
            {
                DataGridFiles[i] = ConvertToDataGridFile(DataGridFiles[i].path, upflag);
            }
        }
        /// <summary>
        /// ファイルのタイムスタンプを変更する
        /// </summary>
        /// <param name="list"></param>
        /// <param name="upflag"></param>
        public bool RenameFromTimestamp(List<DataGridFile> list, UPDATE_FLAG upflag)
        {
            bool exist = false;
            foreach (DataGridFile o in list)
            {
                if (o.isValid && System.IO.File.Exists(o.path))
                {
                    try
                    {
                        DateTime t;
                        if (upflag.HasFlag(UPDATE_FLAG.CREATTION_DATE))
                        {
                            t = File.GetCreationTime(o.path);
                        }
                        else
                        {
                            t = File.GetLastWriteTime(o.path);
                        }
                        var directoryName = Path.GetDirectoryName(o.path);
                        var afterfname = AfterFileName(o.path, t);
                        var afterfullpath = directoryName + @"\" + afterfname;
                        File.Move(o.path, afterfullpath);
                        // 更新日時をファイル名の時刻で更新する
                        o.err_message = "リネーム完了";
                        o.success = true;
                        exist = true;
                    }
                    catch (Exception exception)
                    {
                        //アクセス権限がない
                        o.err_message = exception.Message;
                    }
                }
            }
            return exist;
        }
        /// <summary>
        /// リストからデータ削除
        /// </summary>
        /// <param name="del_list"></param>
        public void RemoveDataGridFile(List<DataGridFile> del_list)
        {
            foreach (DataGridFile o in del_list)
            {
                DataGridFiles.Remove(o);
            }
        }
        /// <summary>
        /// 日付情報を削除したファイル名を返す（フルパス）
        /// </summary>
        /// <param name="fullpath">元のフルパス</param>
        /// <param name="dateMatchString">日付情報の文字列</param>
        /// <returns></returns>
        private string AfterFileName(string fullpath,DateTime dateTime)
        {
            //var dirName = Path.GetDirectoryName(fullpath);
            //var fileName = Path.GetFileName(fullpath);
            //var newFileName = fileName.Replace(dateMatchString, "");
            //return dirName + "\\" + newFileName.Trim();

            var str = dateTime.Year.ToString("D4") + dateTime.Month.ToString("D2")  + dateTime.Day.ToString("D2");
            str += "-" + dateTime.Hour.ToString("D2") + dateTime.Minute.ToString("D2") + dateTime.Second.ToString("D2");

            var newFileName = str + " " + Path.GetFileName(fullpath);
            return newFileName.Trim();
        }

    }
}
