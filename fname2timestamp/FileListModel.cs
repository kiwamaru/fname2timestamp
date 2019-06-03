using DataGridFiles;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace fname2timestamp
{

    /// <summary>
    /// ファイルリスト管理クラス
    /// </summary>
    public class FileListModel : BindableBase
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
        public enum UPDATE_FLAG
        {
            CREATTION_DATE = 1,//作成日時を変更
            UPDATE_DATE = 1 << 1,//更新日時を変更

        };
        public FileListModel()
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
        private List<string> CreateDropFileList(string[] files)
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
        public int DropFileListToDataGridFile(string[] files, UPDATE_FLAG upflag)
        {
            int fcount = 0;
            CurrentProgress = 0;

            List<string> allfiles = CreateDropFileList(files);

            foreach (string f in allfiles)
            {
                CurrentProgress = ((fcount++ * 100) / allfiles.Count);

                DataGridFiles.Add(ConvertToDataGridFile(f, upflag));
            }
            CurrentProgress = 0;
            return DataGridFiles.Count;

        }
        /// <summary>
        /// ファイル名から時刻情報を配列に取り出す
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        private DateTime GetDataTime(string fname)
        {
            //YYYY-MM-DD-mm-dd-ss
            Regex dtimePattern_spl = new Regex(@"(\d+)[\D]+(\d+)[\D]+(\d+)[\D]+(\d+)[\D]+(\d+)(?:[\D]+(\d+)){0,1}", System.Text.RegularExpressions.RegexOptions.Compiled);

            //YYYYMMDD-mmddss
            Regex dtimePattern_seq = new Regex(@"(\d{4})(\d{2})(\d{2})[\D]{0,1}(\d{0,2})(\d{0,2})(\d{0,2})", System.Text.RegularExpressions.RegexOptions.Compiled);
            List<int> dtl = new List<int>();
            Match m_spl = dtimePattern_spl.Match(fname);
            Match m_seq = dtimePattern_seq.Match(fname);
            if (m_spl.Success || m_seq.Success)
            {
                Match m = m_spl.Success ? m_spl : m_seq;
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
                throw new System.ArgumentException("ファイル名に日時情報がありません");
            }
            return new DateTime(dtl[0], dtl[1], dtl[2], dtl[3], dtl[4], dtl[5]); ;
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
            DateTime dt = new DateTime();
            string fname = "";
            string fext = "";
            long fsize = 0;

            System.IO.FileAttributes attr;
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
                    throw new System.UnauthorizedAccessException("ファイルが読み取り専用属性");
                }

                dt = GetDataTime(fname);

                if (upflag.HasFlag(UPDATE_FLAG.CREATTION_DATE) && upflag.HasFlag(UPDATE_FLAG.UPDATE_DATE))
                {
                    if ((dt == System.IO.File.GetCreationTime(fpath)) && (dt == System.IO.File.GetLastWriteTime(fpath)))
                    {
                        throw new System.ArgumentException("既に一致済み");
                    }
                    else
                    {
                        isValid = true;
                    }
                }
                else if (upflag.HasFlag(UPDATE_FLAG.CREATTION_DATE) && (dt == System.IO.File.GetCreationTime(fpath)))
                {
                    throw new System.ArgumentException("既に一致済み");
                }
                else if (upflag.HasFlag(UPDATE_FLAG.UPDATE_DATE) && (dt == System.IO.File.GetLastWriteTime(fpath)))
                {
                    throw new System.ArgumentException("既に一致済み");
                }
                else
                {
                    isValid = true;
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
                name = fname,
                ext = fext,
                update_dtime = System.IO.File.GetLastWriteTime(fpath),
                create_dtime = System.IO.File.GetCreationTime(fpath),
                access_dtime = System.IO.File.GetLastAccessTime(fpath),
                size = fsize,
                f2t_dtime = dt,
                success = false,
                err_message = errmsg,
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
        public bool ChangeTimeStamp(List<DataGridFile> list, UPDATE_FLAG upflag)
        {
            bool exist = false;
            foreach (DataGridFile o in list)
            {
                if (o.isValid && System.IO.File.Exists(o.path))
                {
                    try
                    {
                        if (upflag.HasFlag(UPDATE_FLAG.CREATTION_DATE))
                        {
                            System.IO.File.SetCreationTime(o.path, o.f2t_dtime);//作成日時
                            o.create_dtime = System.IO.File.GetCreationTime(o.path);
                        }
                        if (upflag.HasFlag(UPDATE_FLAG.UPDATE_DATE))
                        {
                            System.IO.File.SetLastWriteTime(o.path, o.f2t_dtime);
                            o.update_dtime = System.IO.File.GetLastWriteTime(o.path);
                        }

                        // 更新日時をファイル名の時刻で更新する
                        o.err_message = "変更完了";
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

    }
}
