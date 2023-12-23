﻿using MaterialDesignThemes.Wpf.Transitions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TransitionEffect = MaterialDesignThemes.Wpf.Transitions.TransitionEffect;
using System.Windows.Media.Imaging;
using System.Threading;
using NSMusicS.UserControlLibrary.MusicPlayer_Main.MusicPlayer_Model_Control_Albums.ViewModel_Assembly_Singer_Show;


namespace NSMusicS.UserControlLibrary.MusicPlayer_Main.MusicPlayer_Model_Control_Singers.ViewModel_Assembly_Singer_Show
{
    public class ViewModel_Assembly_Singer_Class : ViewModelBase
    {
        public class Singer_Info
        {
            public int Singer_No { get; set; }
            public string Singer_Name { get; set; }
            public string Singer_Explain { get; set; }
            public Uri Singer_Image_Uri { get; set; }
            public ImageBrush Singer_Image { get; set; }
            public TransitionEffect Effact { get; set; }

            public double Width { get; set; }
            public double Height { get; set; }
            public Thickness Margin { get; set; }
        }
        public int Num_Singer_Infos { get; set; }//检测是否已完成RelayCommand

        public ViewModel_Assembly_Singer_Class()
        {
            kinds = new List<TransitionEffectKind>
            {
                TransitionEffectKind.ExpandIn,//渐显和展开
                TransitionEffectKind.FadeIn,//逐渐淡入，从完全透明到完全可见
                TransitionEffectKind.SlideInFromLeft,//沿着水平方向从左边滑入
                TransitionEffectKind.SlideInFromTop,
                TransitionEffectKind.SlideInFromRight,
                TransitionEffectKind.SlideInFromBottom
            };
            Singer_Infos = new ObservableCollection<Singer_Info>();
            Num_Singer_Infos = 0;

            /// 一次性全部刷新（一致性）
            RefCommand = new RelayCommand(async () =>
            {
                Singer_Info_Class Singer_Info_Class = Singer_Info_Class.Retuen_This();
                for (int i = 0; i < Singer_Info_Class.Singer_Image_Uris.Count; i++)
                {
                    var existingSinger = Singer_Infos.FirstOrDefault(
                            item => item.Singer_Name.Equals(Singer_Info_Class.Singer_Names[i])
                            );
                    if (existingSinger == null)
                    {
                        Singer_Infos.Add(new Singer_Info()
                        {
                            Singer_No = i,
                            Singer_Name = Singer_Info_Class.Singer_Names[i],
                            Singer_Image = new ImageBrush(new BitmapImage(Singer_Info_Class.Singer_Image_Uris[i])),
                            Singer_Explain = Singer_Info_Class.Singer_Explain[i],
                            Width = 140,
                            Height = 140,
                            Margin = new Thickness(10, 2, 10, 2),
                            Effact = new TransitionEffect()
                            {
                                Kind = kinds[new Random().Next(2, 6)],
                                Duration = new TimeSpan(0, 0, 0, 0, 200)
                            }
                        });
                    }
                    await Task.Delay(1);//单个平滑过渡
                    Num_Singer_Infos++;

                    if (i % 100 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                        }
                    }
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                }
            });
            /// 滚动条多次异步刷新（一致性）
            RefCommand_Async = new RelayCommand(async () =>
            {
                Singer_Info_Class Singer_Info_Class = Singer_Info_Class.Retuen_This();

                for (int i = Singer_Info_Class.Start_Index; i <= Singer_Info_Class.End_Index; i++)
                {
                    if (i >= Singer_Info_Class.Singer_Names.Count || i >= Singer_Info_Class.Singer_Image_Uris.Count)
                        break;

                    if (Singer_Info_Class.Singer_Names[i] != null)
                    {
                        var existingSinger = Singer_Infos.FirstOrDefault(
                            item => item.Singer_Name.Equals(Singer_Info_Class.Singer_Names[i])
                            );
                        if (existingSinger == null)
                        {
                            lock (Singer_Infos)
                            {
                                var SingerInfo = new Singer_Info()
                                {
                                    Singer_No = i,
                                    Singer_Name = Singer_Info_Class.Singer_Names[i],
                                    Singer_Image_Uri = Singer_Info_Class.Singer_Image_Uris[i],
                                    Singer_Image = new ImageBrush(new BitmapImage(Singer_Info_Class.Singer_Image_Uris[i])),
                                    Singer_Explain = Singer_Info_Class.Singer_Explain[i],
                                    Width = 140,
                                    Height = 140,
                                    Margin = new Thickness(10, 2, 10, 2),
                                    Effact = new TransitionEffect()
                                    {
                                        Kind = kinds[new Random().Next(2, 6)],
                                        Duration = new TimeSpan(0, 0, 0, 0, 200)
                                    }
                                };
                                // 添加到队列中
                                AddToQueue(SingerInfo);
                            }
                        }
                    }
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                }
            });
        }

        public RelayCommand RefCommand { get; set; }
        public RelayCommand RefCommand_Async { get; set; }

        public List<TransitionEffectKind> kinds;
        private ObservableCollection<Singer_Info> singer_Infos;
        public ObservableCollection<Singer_Info> Singer_Infos
        {
            get { return singer_Infos; }
            set { singer_Infos = value; RaisePropertyChanged(); }
        }
        //保证数据一致性 + 动画过渡
        private readonly Queue<Singer_Info> SingerInfoQueue = new Queue<Singer_Info>();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        public void AddToQueue(Singer_Info SingerInfo)
        {
            SingerInfoQueue.Enqueue(SingerInfo);
            ProcessQueue();
        }
        private async Task ProcessQueue()
        {
            await semaphore.WaitAsync();

            try
            {
                while (SingerInfoQueue.Count > 0)
                {
                    var SingerInfo = SingerInfoQueue.Dequeue();

                    var existingSinger = Singer_Infos.FirstOrDefault(
                        item => item.Singer_Name.Equals(SingerInfo.Singer_Name)
                        );
                    if (existingSinger == null)
                    {
                        Singer_Infos.Add(SingerInfo);
                        await Task.Delay(40); // 单个平滑过渡
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }




        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        public static ViewModel_Assembly_Singer_Class temp { get; set; }
        public static ViewModel_Assembly_Singer_Class Retuen_This()
        {
            temp = Return_This_Singer_Performer_List_Infos();
            return temp;
        }
        private static ViewModel_Assembly_Singer_Class Return_This_Singer_Performer_List_Infos()
        {
            if (temp == null)
                temp = new ViewModel_Assembly_Singer_Class();
            return temp;
        }

    }
}