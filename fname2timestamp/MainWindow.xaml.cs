using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using DataGridFiles;

namespace fname2timestamp
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<DataGridFile> dataGridFiles;
        //private uint index;
        ProgresBarWindow pbw;

        private int fileCount;

        enum UPDATE_FLAG{
            CREATTION_DATE = 1,
            UPDATE_DATE = 1 << 1,

        };
        public MainWindow()
        {
            InitializeComponent();
            //index = 0;
            dataGridFiles = new ObservableCollection<DataGridFile>();
            dataGridFiles.CollectionChanged += orderlist_CollectionChanged;
            this.dataGrid.ItemsSource = dataGridFiles;
            FilesInfoTextBlock.Text = "↑ファイルをドロップしてください";
            
        }
        void orderlist_CollectionChanged(object sender,System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int listfiles = dataGridFiles.Count;
            FilesInfoTextBlock.Text = listfiles + "個のファイル";

        }
        /**
         * ドロップされたファイルリストからファイルリストを作成する
         * 
         */ 
        private List<string> CreateDropFileList(string[] files)
        {
            var allfiles = new List<string>();
  
            if (files != null)
            {
                foreach (var f in files)
                {
                    //フォルダの場合
                    if (System.IO.Directory.Exists(f))
                    {
                        fileCount += System.IO.Directory.GetFiles(f, "*", System.IO.SearchOption.AllDirectories).Length;
                        //サブフォルダ含めて全ファイルリストを取得
                        IEnumerable<string> d_under_files = System.IO.Directory.EnumerateFiles(f, "*", System.IO.SearchOption.AllDirectories);
                        foreach (var duf in d_under_files)
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
        private List<int> GetDataTimeList(string fname)
        {
            Regex dtimePattern_spl = new Regex(@"^(\d+)[\D]+(\d+)[\D]+(\d+)[\D]+(\d+)[\D]+(\d+)(?:[\D]+(\d+)){0,1}", System.Text.RegularExpressions.RegexOptions.Compiled);
            Regex dtimePattern_seq = new Regex(@"^(\d{4})(\d{2})(\d{2})[\D]{0,1}(\d{0,2})(\d{0,2})(\d{0,2})", System.Text.RegularExpressions.RegexOptions.Compiled);
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
            return dtl;
        }
        private DataGridFile convertDataGridFile(string fpath,UPDATE_FLAG upflag)
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
                System.IO.FileInfo  fi = new System.IO.FileInfo(fpath);
                fsize = fi.Length;
                attr = System.IO.File.GetAttributes(fpath);
                fname = System.IO.Path.GetFileName(fpath);
                fext = System.IO.Path.GetExtension(fpath);
                //読み取り専用属性があるか調べる
                if ((attr & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
                {
                    throw new System.UnauthorizedAccessException("ファイルが読み取り専用属性");
                }

                List<int> dtl = GetDataTimeList(fname);
                if (dtl.Count == 6)
                {
                    if (dtl[0] > 2099 || dtl[0] < 1970)
                    {
                        throw new System.ArgumentException("年が異常値 1970-2099まで");
                    }
                    dt = new DateTime(dtl[0], dtl[1], dtl[2], dtl[3], dtl[4], dtl[5]);

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
                else
                {
                    throw new System.ArgumentException("日時情報解析エラー");
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
        private void Window_Drop(object sender, DragEventArgs e)
        {
            UPDATE_FLAG upflag = 0;
            if (updateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.UPDATE_DATE;
            }
            if (creationDateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.CREATTION_DATE;
            }
            pbw = new ProgresBarWindow();
            //var allfiles = new List<string>();
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            var allfiles = CreateDropFileList(files);
            if (allfiles.Count >= 50)
            {
                pbw.Show();
                pbw.Topmost = true;
            }
            int fcount = 0;
                
            foreach (var f in allfiles)
            {
                int per = ((fcount*100) / allfiles.Count);
                pbw.FileRegexProgress(per);

                dataGridFiles.Add(convertDataGridFile(f, upflag));
                fcount++;
            }
            pbw.Close();

        }
        /**
         * リストをオプションに従いリロードする
         */
        private void reloadList(UPDATE_FLAG upflag)
        {
            List<DataGridFile> all_list = dataGrid.Items.Cast<DataGridFile>().ToList();
            for (int i=0; i < all_list.Count;i++ )
            {
                all_list[i].isValid = convertDataGridFile(all_list[i].path, upflag).isValid;
                all_list[i].err_message = convertDataGridFile(all_list[i].path, upflag).err_message;
                all_list[i].success = false;
            }
/*            dataGridFiles.Clear();
            foreach (var o in all_list)
            {
                if(System.IO.File.Exists(o.path)){
                    dataGridFiles.Add(convertDataGridFile(o.path, upflag));
                }
            }
            */
        }
        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            // ファイルをドロップされた場合のみ e.Handled を True にする
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }
        /**
         * ファイルのタイムスタンプを変更する 
         */
        private void updateDataGirdFile(List<DataGridFile> list, UPDATE_FLAG upflag)
        {
            bool exist = false;
            foreach (var o in list)
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
            if(!exist)
            {
                MessageBox.Show("時刻変換可能なファイルがリストにありません","エラー",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
        /**
         * 選択されたデータを変換
         */
        private void btnSelExec_Click(object sender, RoutedEventArgs e)
        {
            UPDATE_FLAG upflag = 0;
            if (updateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.UPDATE_DATE;
            }
            if (creationDateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.CREATTION_DATE;
            }

            List<DataGridFile> sel_list = dataGrid.SelectedItems.Cast<DataGridFile>().ToList();
            updateDataGirdFile(sel_list, upflag);
            dataGrid.SelectedIndex = -1;
        }

        /**
         * 有効な全データを変換
         */
        private void btnAllExec_Click(object sender, RoutedEventArgs e)
        {
            UPDATE_FLAG upflag = 0;
            if (updateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.UPDATE_DATE;
            }
            if (creationDateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.CREATTION_DATE;
            }

            List<DataGridFile> all_list = dataGrid.Items.Cast<DataGridFile>().ToList();
            updateDataGirdFile(all_list, upflag);
            dataGrid.SelectedIndex = -1;
        }
        /**
         * 選択されたデータを変換リストから削除
         */
        private void btnSelDel_Click(object sender, RoutedEventArgs e)
        {
            List<DataGridFile> sel_list = dataGrid.SelectedItems.Cast<DataGridFile>().ToList();

            foreach(var o in sel_list)
            {
                dataGridFiles.Remove(o);
            }
            dataGrid.SelectedIndex = -1;
        }
        /**
         * 全データを変換リストから削除
         */
        private void btnAllDel_Click(object sender, RoutedEventArgs e)
        {
            dataGridFiles.Clear();
            dataGrid.SelectedIndex = -1;
        }
        /**
         * 選択ボタンの有効・無効を切り替える
         */
        private void DG_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            //Get the newly selected cells
            List<DataGridFile> sel_list = dataGrid.SelectedItems.Cast<DataGridFile>().ToList();
            bool can_exe = false;
            foreach (var x in sel_list)
            {
                if (x.isValid == true)
                {
                    can_exe = true;
                }
            }
            if (can_exe) {
                this.btnSelExec.IsEnabled = true;
            }
            else
            {
                this.btnSelExec.IsEnabled = false;
            }
            if (sel_list.Count >= 1)
            {
                this.btnSelDel.IsEnabled = true;
            }
            else
            {
                this.btnSelDel.IsEnabled = false;
            }
        }
        /**
         * 作成日時変更チェックボックスのクリックイベント
         */
        private void creationDateChcekBox_Checked(object sender, RoutedEventArgs e)
        {
            
            UPDATE_FLAG upflag = 0;
            if (updateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.UPDATE_DATE;
            }
            if (creationDateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.CREATTION_DATE;
            }
            if (upflag == 0)
            {
                creationDateChcekBox.IsChecked = true;
                MessageBox.Show("両方共外すことは出来ません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            reloadList(upflag);
            dataGrid.SelectedIndex = -1;
        }

        /**
         * 更新日時変更チェックボックスのクリックイベント
         */
        private void updateChcekBox_Checked(object sender, RoutedEventArgs e)
        {
            UPDATE_FLAG upflag = 0;
            if (updateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.UPDATE_DATE;
            }
            if (creationDateChcekBox.IsChecked == true)
            {
                upflag |= UPDATE_FLAG.CREATTION_DATE;
            }
            if (upflag == 0)
            {
                updateChcekBox.IsChecked = true;
                MessageBox.Show("両方共外すことは出来ません", "エラー", MessageBoxButton.OK, MessageBoxImage.Information); 
                return;
            }
            reloadList(upflag);
            dataGrid.SelectedIndex = -1;
        }

    }

}
