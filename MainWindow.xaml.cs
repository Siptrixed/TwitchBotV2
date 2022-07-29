using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using TwitchBotV2.Model;
using TwitchBotV2.Model.DataModel;
using TwitchBotV2.Model.Localhost;
using TwitchBotV2.Model.Twitch;
using TwitchBotV2.Model.Twitch.EventArgs;
using TwitchBotV2.Model.Twitch.Utils;
using TwitchBotV2.Model.UserScript;
using TwitchBotV2.Model.UserScript.Actions;
using TwitchBotV2.Model.Utils;
using TwitchBotV2.Model.WinApi;

namespace TwitchBotV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Construct
        public TwitchClient Client;
        private TaskbarIcon tbi = new TaskbarIcon();
        public MainWindow()
        {
            InitializeComponent();
            WebServer.RunServer();
            tbi.Icon = Properties.Resources.Icon;
            tbi.ToolTipText = "TwitchBotV2";
            tbi.TrayLeftMouseUp += (x, y) => { Show(); Activate(); };
            Application.Current.Exit += (x, y) => tbi.Dispose();
            Settings_MinimizeToTray.IsChecked = GlobalModel.Settings.MinimizeToTray;
            RewardScriptActionEditGrid.Visibility = Reward_ScriptActions.Visibility 
                = RewardEditGrid.Visibility = Visibility.Hidden;
            VersionLabel.Content = $"v{VersionControl.Version}";
            FullWindowBanner.Visibility = PleaseWaitGrid.Visibility = Visibility.Visible;
            HotkeySelector1.Text = GlobalModel.Settings.ISSHotkey?.ToString();
            Activate();
            Task.Run(async () => {
                var version = await VersionControl.CheckVersion();
                if (version != null)
                {
                    var success = await VersionControl.DownloadUpdateAsync(version);
                    if (success)
                    {
                        if (!VersionControl.UpdateNow())
                        {
                            MyAppExt.InvokeUI(() =>
                            {
                                FullWindowBanner.Visibility = PleaseWaitGrid.Visibility = Visibility.Collapsed;
                                InitAuthorization();
                            });
                        }
                    }
                    else
                    {
                        MyAppExt.InvokeUI(() =>
                        {
                            FullWindowBanner.Visibility = PleaseWaitGrid.Visibility = Visibility.Collapsed;
                            InitAuthorization();
                        });
                    }
                }
                else
                {
                    MyAppExt.InvokeUI(() =>
                    {
                        FullWindowBanner.Visibility = PleaseWaitGrid.Visibility = Visibility.Collapsed;
                        InitAuthorization();
                    });
                }
            });
        }
        #endregion

        #region BotLogic
        private void InitBotLogic()
        {
            UpdateRewardButtonClick(this, null);
        }

        private void TwitchRewardRedeemed(object? sender, RewardEventArgs e)
        {
            if (GlobalModel.Settings.CustomRewards.ContainsKey(e.CustomRewardID))
            {
                var script = GlobalModel.Settings.CustomRewards[e.CustomRewardID].Script;
                script.Invoke(Client, e);
            }
        }

        private void TwitchMessageRemoved(object? sender, MessageRemovedEventArgs e)
        {
            foreach (var redeem in MyCallableUserScript.Queue)
            {
                if (redeem.LinkedMessage != null && redeem.LinkedMessage.ID == e.MessageID)
                {
                    redeem.IsRemoved = true;
                    break;
                }
            }
        }

        private void TwitchMessageNew(object? sender, MessageRecivedEventArgs e)
        {
            foreach (var redeem in MyCallableUserScript.Queue)
            {
                if (redeem.CustomRewardID == e.CustomRewardID && e.UserID == redeem.UserID && redeem.LinkedMessage == null)
                {
                    redeem.LinkedMessage = e;
                    break;
                }
            }
            if (e.Message == "Ping") Client.SendMessage("Pong");
        }

        private async Task UpdateRewards()
        {
            var allRewards = await Client.Account.GetRewards();
            var toRemove = GlobalModel.Settings.CustomRewards.Keys.ToList();
            foreach (var reward in allRewards.List())
            {
                if (string.IsNullOrWhiteSpace(reward["id"])) continue;
                if (!GlobalModel.Settings.CustomRewards.ContainsKey(reward["id"]))
                {
                    GlobalModel.Settings.CustomRewards.Add(reward["id"], new Model.DataModel.MyRewardInfo());
                }
                else toRemove.Remove(reward["id"]);
                var rewardInfo = GlobalModel.Settings.CustomRewards[reward["id"]];
                rewardInfo.ID = reward["id"];
                rewardInfo.Cost = reward["cost"];
                rewardInfo.Title = reward["title"];
                rewardInfo.Prompt = reward["prompt"];
                rewardInfo.RequaredApply = !reward["should_redemptions_skip_request_queue"];
            }
            foreach (var remId in toRemove)
            {
                GlobalModel.Settings.CustomRewards.Remove(remId);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            MyAppExt.InvokeUI(() => { Reward_list.ItemsSource = GlobalModel.Settings.CustomRewards; RewardListUpdateButton.IsEnabled = true; });
        }

        private void UpdateRewardButtonClick(object sender, RoutedEventArgs e)
        {
            Reward_list.ItemsSource = null;
            RewardListUpdateButton.IsEnabled = false;
            Task.Run(async () =>
            {
                await UpdateRewards();
            });
        }
        private void UpdateRewardSector(string? rewardId = null, MyRewardInfo? reward = null)
        {
            if (!string.IsNullOrEmpty(rewardId))
            {
                Reward_Title.Content = reward.Title;
                Reward_Prompt.Text = reward.Prompt;
                Reward_Cost.Content = $"{reward.Cost}cp";
                Reward_ScriptActions.ItemsSource = reward.Script.Actions;
                Reward_ScriptActions.Visibility = RewardEditGrid.Visibility = Visibility.Visible;
                Reward_ExecuteAsync.IsChecked = reward.Script.IsAsync;
                RewardSkipQueueAttention.Visibility = reward.RequaredApply ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                Reward_Cost.Content= Reward_Title.Content = Reward_Prompt.Text = "";
                Reward_ExecuteAsync.IsChecked = false;
                Reward_ScriptActions.ItemsSource = null;
                Reward_ScriptActions.Visibility = RewardEditGrid.Visibility = Visibility.Hidden;
                RewardSkipQueueAttention.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Authorization
        private void InitAuthorization()
        {
            GlobalModel.TwithcAccountAuthorized += Authorized;
            GlobalModel.TwithcClientInitialized += TwitchInitialized;
            if (GlobalModel.Settings.TwitchToken != null)
            {
                Authorized(this, GlobalModel.Settings.TwitchToken);
            }
            else
            {
                LoginGrid.Visibility = Visibility.Visible;
            }
            FullWindowBanner.Visibility = Visibility.Visible;
        }
        private void TwitchInitialized(object? sender, bool e)
        {
            if (e)
            {
                MyAppExt.InvokeUI(() =>
                {
                    FullWindowBanner.Visibility = PleaseWaitGrid.Visibility = Visibility.Collapsed;
                    InitBotLogic();
                    MainTabControl.SelectedIndex = 0;
                });
            }
            else Process.Start(new ProcessStartInfo(TwitchConsts.GetAuthURL()) { UseShellExecute = true });
        }
        private void Authorized(object? sender, string e)
        {
            MyAppExt.InvokeUI(() =>
            {
                Activate();
                LoginGrid.Visibility = Visibility.Collapsed;
                PleaseWaitGrid.Visibility = Visibility.Visible;
                Task.Run(async () =>
                {
                    Client = await TwitchClient.Construct(e);
                    Client.MessageRecived += TwitchMessageNew;
                    Client.MessageRemoved += TwitchMessageRemoved;
                    Client.RewardRedeemed += TwitchRewardRedeemed;
                });
            });
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(TwitchConsts.GetAuthURL()) { UseShellExecute = true });
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GlobalModel.Settings.TwitchToken = null;
            Application.Current.Shutdown();
        }
        #endregion

        #region WindowLogic
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(this);
                if (pos.Y < 21) this.DragMove();
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (GlobalModel.Settings.MinimizeToTray)
            {
                Hide();
            }
            else WindowState = WindowState.Minimized;
        }
        private void Reward_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                if (e.AddedItems[0] is KeyValuePair<string, MyRewardInfo> selectedItem)
                {
                    UpdateRewardSector(selectedItem.Key, selectedItem.Value);
                    return;
                }
            }
            UpdateRewardSector();
        }

        private void Settings_MinimizeToTray_Click(object sender, RoutedEventArgs e)
        {
            GlobalModel.Settings.MinimizeToTray = Settings_MinimizeToTray.IsChecked ?? false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Reward_ActionAdder.SelectedIndex != -1)
            {
                if (Reward_list.SelectedItem is KeyValuePair<string, MyRewardInfo> RewInf)
                {
                    Reward_ScriptActions.ItemsSource = null;
                    MyScriptActionBase NewAction;
                    switch (Reward_ActionAdder.SelectedIndex)
                    {
                        case 0:
                            NewAction = new DelayAction(900);
                            break;
                        case 1:
                            NewAction = new TextActions("Привет");
                            break;
                        case 2:
                            NewAction = new TextActions("echo привет", MyScriptActionType.ShellComand);
                            break;
                        case 3:
                            NewAction = new AudioActions("{redeem.text}", MyScriptActionType.Speech);
                            break;
                        case 4:
                            NewAction = new AudioActions("{redeem.text}", MyScriptActionType.SpeechTrueTTS);
                            break;
                        case 5:
                            NewAction = new AudioActions("C:/", MyScriptActionType.PlayAudio);
                            break;
                        default:
                            NewAction = new DelayAction(0);
                            break;
                    }
                    RewInf.Value.Script.Actions.Add(NewAction);
                    Reward_ScriptActions.ItemsSource = RewInf.Value.Script.Actions;
                    Reward_ScriptActions.SelectedIndex = RewInf.Value.Script.Actions.Count - 1;
                }
                Reward_ActionAdder.SelectedIndex = -1;
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Reward_ActionAdder.IsDropDownOpen = true;
        }

        private void Reward_ExecuteAsync_Click(object sender, RoutedEventArgs e)
        {
            if (Reward_list.SelectedItem is KeyValuePair<string, MyRewardInfo> RewInf)
            {
                RewInf.Value.Script.IsAsync = Reward_ExecuteAsync.IsChecked ?? false;
            }
        }

        private void Reward_ScriptActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (Reward_ScriptActions.SelectedIndex != -1)
            {
                UpdateActionEdit(Reward_ScriptActions.SelectedItem as MyScriptActionBase);
                RewardScriptActionEditGrid.Visibility = Visibility.Visible;
                RewardActionDownButton.IsEnabled = RewardActionUpButton.IsEnabled = true;
            }
            else
            {
                UpdateActionEdit();
                RewardScriptActionEditGrid.Visibility = Visibility.Hidden;
                RewardActionDownButton.IsEnabled = RewardActionUpButton.IsEnabled = false;
            }
        }

        private void RewardActionUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (Reward_ScriptActions.SelectedIndex > 0)
            {
                if (Reward_list.SelectedItem is KeyValuePair<string, MyRewardInfo> selectedItem)
                {
                    int index = Reward_ScriptActions.SelectedIndex;
                    Reward_ScriptActions.ItemsSource = null;
                    var moingItem = selectedItem.Value.Script.Actions[index - 1];
                    selectedItem.Value.Script.Actions[index - 1] = selectedItem.Value.Script.Actions[index];
                    selectedItem.Value.Script.Actions[index] = moingItem;
                    Reward_ScriptActions.ItemsSource = selectedItem.Value.Script.Actions;
                    Reward_ScriptActions.SelectedIndex = index - 1;
                }
            }
        }

        private void RewardActionDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (Reward_list.SelectedItem is KeyValuePair<string, MyRewardInfo> selectedItem)
            {
                int index = Reward_ScriptActions.SelectedIndex;
                if (Reward_ScriptActions.SelectedIndex != -1 && selectedItem.Value.Script.Actions.Count > index + 1)
                {
                    Reward_ScriptActions.ItemsSource = null;
                    var moingItem = selectedItem.Value.Script.Actions[index + 1];
                    selectedItem.Value.Script.Actions[index + 1] = selectedItem.Value.Script.Actions[index];
                    selectedItem.Value.Script.Actions[index] = moingItem;
                    Reward_ScriptActions.ItemsSource = selectedItem.Value.Script.Actions;
                    Reward_ScriptActions.SelectedIndex = index + 1;
                }
            }
        }

        private void RewardActionDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (Reward_list.SelectedItem is KeyValuePair<string, MyRewardInfo> selectedItem)
            {
                int index = Reward_ScriptActions.SelectedIndex;
                if (Reward_ScriptActions.SelectedIndex != -1 && selectedItem.Value.Script.Actions.Count > index)
                {
                    Reward_ScriptActions.ItemsSource = null;
                    selectedItem.Value.Script.Actions.RemoveAt(index);
                    Reward_ScriptActions.ItemsSource = selectedItem.Value.Script.Actions;
                    Reward_ScriptActions.SelectedIndex = selectedItem.Value.Script.Actions.Count > index? index: index-1;
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Reward_ScriptActions != null)
            UpdateActionEdit(Reward_ScriptActions.SelectedItem as MyScriptActionBase);
        }
        private void UpdateActionEdit(MyScriptActionBase? action = null)
        {
            if(action != null)
            {
                switch (action.Type)
                {
                    case MyScriptActionType.Timer:
                        if (action is DelayAction delay)
                        {
                            RewardActionCaption.Content = "Ожидать (миллисекунд)";
                            RewardDelayActionText.Text = delay.Delay.ToString();
                            RewardActionEditTypeTab.SelectedIndex = 1;
                        }
                        break;
                    case MyScriptActionType.SendMessage:
                        if (action is TextActions sendMessage)
                        {
                            RewardActionCaption.Content = "Сообщение";
                            RewardTextActionText.Text = sendMessage.Text;
                            RewardActionEditTypeTab.SelectedIndex = 0;
                        }
                        break;
                    case MyScriptActionType.PlayAudio:
                        if (action is AudioActions audio2Message)
                        {
                            RewardActionCaption.Content = "Аудио файл";
                            RewardAudioActionText.Text = audio2Message.Text;
                            RewardAudioActionVolume.Value = audio2Message.Volume;
                            RewardAudioActionText.IsEnabled = false;
                            RewardAudioActionText.Height = 20;
                            RewardAudioActionFileSelector.Visibility = Visibility.Visible;
                            RewardActionEditTypeTab.SelectedIndex = 2;
                            RewardActionVoiceGrid.Visibility = Visibility.Collapsed;
                        }
                        break;
                    case MyScriptActionType.Speech:
                        if (action is AudioActions audio0Message)
                        {
                            RewardActionCaption.Content = "Текст для синтеза речи";
                            RewardAudioActionText.Text = audio0Message.Text;
                            RewardAudioActionVolume.Value = audio0Message.Volume;
                            RewardAudioActionText.IsEnabled = true;
                            RewardAudioActionText.Height = 36;
                            RewardAudioActionFileSelector.Visibility = Visibility.Collapsed;
                            RewardActionEditTypeTab.SelectedIndex = 2;
                            RewardActionVoice.Visibility = Visibility.Hidden;
                            RewardActionVoiceGrid.Visibility = Visibility.Visible;
                            RewardAudioActionRate.Value = audio0Message.Rate;
                        }
                        break;
                    case MyScriptActionType.SpeechTrueTTS:
                        if (action is AudioActions audio1Message)
                        {
                            RewardActionCaption.Content = "Текст для озвучивания";
                            RewardAudioActionText.Text = audio1Message.Text;
                            RewardAudioActionVolume.Value = audio1Message.Volume;
                            RewardAudioActionText.IsEnabled = true;
                            RewardAudioActionText.Height = 36;
                            RewardAudioActionFileSelector.Visibility = Visibility.Collapsed;
                            RewardActionEditTypeTab.SelectedIndex = 2;
                            RewardActionVoice.Visibility = Visibility.Visible;
                            RewardActionVoice.SelectedIndex = (int)audio1Message.Voice;
                            RewardActionVoiceGrid.Visibility = Visibility.Visible;
                            RewardAudioActionRate.Value = audio1Message.Rate;
                        }
                        break;
                    case MyScriptActionType.ShellComand:
                        if (action is TextActions shellComand)
                        {
                            RewardActionCaption.Content = "Командная строка";
                            RewardTextActionText.Text = shellComand.Text;
                            RewardActionEditTypeTab.SelectedIndex = 0;
                        }
                        break;
                    
                    default:
                        RewardActionEditTypeTab.SelectedIndex = -1;
                        break;
                }
            }
            else
            {
                RewardActionEditTypeTab.SelectedIndex = -1;
            }
        }

        private void RewardTextActionText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (Reward_ScriptActions?.SelectedItem is TextActions act)
                {
                    act.Text = textBox.Text;
                }
            }
        }

        private void RewardActionSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Reward_list.SelectedItem is KeyValuePair<string, MyRewardInfo> selectedItem)
            {
                int selected = Reward_ScriptActions.SelectedIndex;
                Reward_ScriptActions.ItemsSource = null;
                Reward_ScriptActions.ItemsSource = selectedItem.Value.Script.Actions;
                Reward_ScriptActions.SelectedIndex = selected;
            }
        }

        private void RewardDelayActionText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (Reward_ScriptActions?.SelectedItem is DelayAction act)
                {
                    int.TryParse(textBox.Text, out int result);
                    if (result < 0) result = 0;
                    act.Delay = result;
                    int c = textBox.CaretIndex;
                    textBox.Text = act.Delay.ToString();
                    textBox.CaretIndex = c;
                    RewardDelayActionTimeCaption.Content = $"Ждать {MyAppExt.GetTimeFromMilliseconds(result)}";
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                int.TryParse(RewardDelayActionText.Text, out int result);
                
                int c = RewardDelayActionText.CaretIndex;
                switch (button.Content.ToString())
                {
                    case "+1min": result += 60000; break;
                    case "-1min": result -= 60000; break;
                    case "+1s": result += 1000; break;
                    case "-1s": result -= 1000; break;
                    case "+100ms": result += 100;break;
                    case "-100ms": result -= 100; break;
                }
                if (result < 0) result = 0;
                RewardDelayActionText.Text = result.ToString();
                RewardDelayActionText.CaretIndex = c;
                RewardDelayActionTimeCaption.Content = $"Ждать: {MyAppExt.GetTimeFromMilliseconds(result)}";
                RewardActionSaveButton_Click(this, null);
            } }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (Reward_ScriptActions?.SelectedItem is AudioActions act)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Аудио файл (*.mp3;*.wav)|*.mp3;*.wav";
                if (openFileDialog.ShowDialog() == true)
                    act.Text = RewardAudioActionText.Text = openFileDialog.FileName;
                RewardActionSaveButton_Click(this,null);
            }
        }

        private void RewardAudioActionVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Reward_ScriptActions?.SelectedItem is AudioActions act)
            {
                act.Volume = (byte)e.NewValue;
            }
        }

        private void RewardAudioActionText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (Reward_ScriptActions?.SelectedItem is AudioActions act)
                {
                    act.Text = textBox.Text;
                }
            }
        }

        private void RewardAudioActionRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Reward_ScriptActions?.SelectedItem is AudioActions act)
            {
                act.Rate = (int)e.NewValue;
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (Reward_ScriptActions?.SelectedItem is AudioActions act)
            {
                act.Voice = (TrueTTSVoices)RewardActionVoice.SelectedIndex;
            }
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) keyMode = 2;
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) keyMode = 1;
        }
        int keyMode = -1;
        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift
            || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl
            || e.Key == Key.LeftAlt || e.Key == Key.RightAlt) keyMode = -1;
            else
            {
                Key key = e.Key;
                if (key == Key.System)
                {
                    key = e.SystemKey;
                    keyMode = 0;
                }
                else if (keyMode == 0)
                    keyMode = -1;

                string mode = "";
                try
                {
                    switch (keyMode)
                    {
                        case 0:
                            mode = "Alt+";
                            GlobalModel.SetISSHotkey(key, KeyModifier.Alt);
                            break;
                        case 1:
                            mode = "Ctrl+";
                            GlobalModel.SetISSHotkey(key, KeyModifier.Ctrl);
                            break;
                        case 2:
                            mode = "Shift+";
                            GlobalModel.SetISSHotkey(key, KeyModifier.Shift);
                            break;
                        default:
                            GlobalModel.SetISSHotkey(key, KeyModifier.None);
                            break;
                    }
                    ((TextBox)sender).Text = (mode) + key;
                }
                catch
                {

                }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            GlobalModel.ClearISSHotKey();
            HotkeySelector1.Text = "";
        }
    }
    #endregion
}
