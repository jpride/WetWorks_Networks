﻿using System;
using System.Windows;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Animation;




namespace WetWorks_NetWorks
{ 
    public partial class MainWindow : Window
    {
        #region ******** Variables ********

        public Boolean debug = true;


        public int choiceSelect = 0;

        public bool isEthernetSelected = false;
        public bool isWifiSelected = false;

        public int selectChoiceInt = 0;

        private static System.Windows.Forms.Timer _adapterCheckTimer;

        private static System.Timers.Timer _networkChangeDebounceTimer;
        private static readonly int _networkChangeDebounceTimerInterval = 10000;

        private NetworkInterface _nic;
        private string _adapterName;
        private long _speed;
        private int _adapterCount = 0;
        private NetworkInterfaceType _adapterType;

        readonly string _dhcpChoiceContent = "DHCP";
        readonly string _choice2Content = "10.10.1.253 / 16";
        readonly string _choice3Content = "192.168.1.253 / 24";
        readonly string _choice4Content = "169.254.1.253 / 16";

        readonly string _choice2Address = "10.10.1.253";
        readonly string _choice3Address = "192.168.1.253";
        readonly string _choice4Address = "169.254.1.253";

        readonly string _dhcpNetShChoiceString = "dhcp";
        readonly string _choice2NetShString = "static address=10.10.1.253 mask=255.255.0.0 gateway=10.10.1.1";
        readonly string _choice3NetShString = "static address=192.168.1.253 mask=255.255.255.0 gateway=192.168.1.1";
        readonly string _choice4NetShString = "static address=169.254.1.253 mask=255.255.0.0 gateway=169.254.1.1";

        UserIpChoice Choice1;
        UserIpChoice Choice2;
        UserIpChoice Choice3;
        UserIpChoice Choice4;


        #endregion 

        public MainWindow()
        {
            //Build Window
            InitializeComponent();

            //Event handler watching for Network Address Change, triggers the UpdateAdapterInfo() method
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkAvailabilityChangedCallback);


            //Initialize UserIpChoice objects
            Choice1 = new UserIpChoice(_dhcpChoiceContent, String.Empty, _dhcpNetShChoiceString);
            Choice2 = new UserIpChoice(_choice2Content, _choice2Address, _choice2NetShString);
            Choice3 = new UserIpChoice(_choice3Content, _choice3Address, _choice3NetShString);
            Choice4 = new UserIpChoice(_choice4Content, _choice4Address, _choice4NetShString);

            //Set Button Content from UserIpChoice objects
            choice1Btn.Content = Choice1.UiContent;
            choice2Btn.Content = Choice2.UiContent;
            choice3Btn.Content = Choice3.UiContent;
            choice4Btn.Content = Choice4.UiContent;


            //get assembly version for version label
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            assemblyLbl.Content = String.Format($"v{version}");

            //intialize adapter info
            GetAdapterInfoAtStartup();
        }


        #region ******** ButtonEvents ********
        private void choiceBtn_Click(object sender, RoutedEventArgs e)
        {
            RadioButton myButton = (RadioButton)sender;
            string myName = myButton.Name;

            switch (myButton.Name)
            {
                case "choice1Btn":
                    choiceSelect = 1;
                    ChoiceInterlock(choiceSelect);
                    ResetUserEntryText();

                    if (choiceSelect == 1)
                    {
                        Process p = CreateProcess(_adapterName, _dhcpNetShChoiceString);
                        ProcessRequest(p);

                        UpdateStatusLbl("waiting for DHCP...");
                        SetIpaText(String.Empty);

                        _adapterCheckTimer = new System.Windows.Forms.Timer();
                        _adapterCheckTimer.Tick += new EventHandler(CheckAdapterAfterTimeOut);
                        _adapterCheckTimer.Interval = 5000;
                        _adapterCheckTimer.Start();
                        UpdateStatusLbl("Waiting for active adapter");
                    }
                    break;
                case "choice2Btn":
                    choiceSelect = 2;
                    ChoiceInterlock(choiceSelect);
                    ResetUserEntryText();

                    if (choiceSelect == 2)
                    {                      
                        Process p = CreateProcess(_adapterName, _choice2NetShString);
                        ProcessRequest(p);
                        UpdateAdapterInfo();
                    }
                    break;
                case "choice3Btn":
                    choiceSelect = 3;
                    ChoiceInterlock(choiceSelect);
                    ResetUserEntryText();

                    if (choiceSelect == 3)
                    {
                        Process p = CreateProcess(_adapterName, _choice3NetShString);
                        ProcessRequest(p);
                        UpdateAdapterInfo();
                    }
                    break;
                case "choice4Btn":
                    choiceSelect = 4;
                    ChoiceInterlock(choiceSelect);
                    ResetUserEntryText();

                    if (choiceSelect == 4)
                    {
                        Process p = CreateProcess(_adapterName, _choice4NetShString);
                        ProcessRequest(p);
                        UpdateAdapterInfo();
                    }
                    break;
                case "choice5Btn":
                    choiceSelect = 5;
                    ChoiceInterlock(choiceSelect);
                    UpdateStatusLbl(String.Empty);
                    break;

                default:
                    break;
            }
        }

        private void ChoiceInterlock(int index)
        {
            choiceSelect = index;

            this.Dispatcher.Invoke(() => 
            {
                choice1AccentBtn.Visibility = Visibility.Hidden;
                choice2AccentBtn.Visibility = Visibility.Hidden;
                choice3AccentBtn.Visibility = Visibility.Hidden;
                choice4AccentBtn.Visibility = Visibility.Hidden;
                choice5AccentBtn.Visibility = Visibility.Hidden;
            });


            switch (index)
            {
                case 1:
                    this.Dispatcher.Invoke(() => 
                    {
                        //FadeInAndOut(choice1AccentBtn);
                        choice1AccentBtn.Visibility = Visibility.Visible;
                        choice1Btn.IsChecked = true;
                        ResetUserEntryText();
                    });

                    break;
                case 2:
                    this.Dispatcher.Invoke(() =>
                    {
                        //FadeInAndOut(choice2AccentBtn);
                        choice2AccentBtn.Visibility = Visibility.Visible;
                        choice2Btn.IsChecked = true;
                        ResetUserEntryText();
                    });
                    break;
                case 3:
                    this.Dispatcher.Invoke(() =>
                    {
                        //FadeInAndOut(choice3AccentBtn);
                        choice3AccentBtn.Visibility = Visibility.Visible;
                        choice3Btn.IsChecked = true;
                        ResetUserEntryText();
                    });
                    break;
                case 4:
                    this.Dispatcher.Invoke(() =>
                    {
                        
                        choice4AccentBtn.Visibility = Visibility.Visible;
                        choice4Btn.IsChecked = true;
                        //FadeInAndOut(choice4AccentBtn);
                        ResetUserEntryText();
                    });
                    break;
                case 5:
                    this.Dispatcher.Invoke(() =>
                    {
                        //FadeInAndOut(choice5AccentBtn);
                        choice5AccentBtn.Visibility = Visibility.Visible;
                        choice5Btn.IsChecked = true;
                    });
                    break;
                default:
                    break;
            }

        }

        private void userEntry_GotFocus(object sender, RoutedEventArgs e)
        {
            choiceSelect = 5;
            ChoiceInterlock(choiceSelect);
        }

        private void LogoBtn_Click(object sender, RoutedEventArgs e)
        {
            string exePath = "C:\\windows\\system32\\control.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "/name Microsoft.NetworkAndSharingCenter",
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true,
            };

            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
        }

        private void EthernetBtn_Click(object sender, RoutedEventArgs e)
        {
            isEthernetSelected = true;
            ResetUserEntryText();
            _adapterType = NetworkInterfaceType.Ethernet;
            UpdateAdapterInfo();
        }

        private void WifiBtn_Click(object sender, RoutedEventArgs e)
        {
            isWifiSelected = true;
            ResetUserEntryText();
            _adapterType = NetworkInterfaceType.Wireless80211;
            UpdateAdapterInfo();
        }
        #endregion

        
        #region ******** Network Functions ********
        private void AddressChangedCallback(object sender, EventArgs e)
        {
            Console.WriteLine($"AddressChangedCallback Entered");

            //Implemented a debounce timer to account for the fact that this event gets called several times for a single network change.
            // Stop any existing timer
            _networkChangeDebounceTimer?.Stop();

            // Create a new timer if it doesn't exist
            if (_networkChangeDebounceTimer == null)
            {
                _networkChangeDebounceTimer = new System.Timers.Timer { Interval = _networkChangeDebounceTimerInterval };
                _networkChangeDebounceTimer.AutoReset = false;
                _networkChangeDebounceTimer.Elapsed += (s, args) =>
                {
                    _networkChangeDebounceTimer.Stop();               
                    
                    //Refresh apapters and find the one that matches _adapter from UpdateAdapterInfo()
                    NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface n in adapters)
                    {
                        if (n.Name == _adapterName)
                        {
                            _nic = n;
                            break;
                        }
                    }

                    //Run UpdateAdapterInfo() if the adapter is Up, if not, place message in label that reports it down
                    if (_nic.OperationalStatus == OperationalStatus.Up)
                    {
                        try
                        {
                            UpdateAdapterInfo();
                        }
                        catch (Exception ex)
                        {
                            Console.Write("Error in AddressChangedCallback: {0}", ex.Message);
                        }
                    }
                    else
                    {
                        UpdateStatusLbl(String.Format($"{_adapterName} is down..."));
                        SetIpaText(String.Empty);
                        SetSpeed();
                    }
                }; 
            }

            // Start the timer
            _networkChangeDebounceTimer.Start();
        }

        private void NetworkAvailabilityChangedCallback(object sender, NetworkAvailabilityEventArgs e)
        {
            Console.WriteLine($"NetworkAvailabilityChangedCallback Entered");
            Console.WriteLine($"{sender} availibility: {e.IsAvailable}");

            //Refresh apapters and find the one that matches _adapter from UpdateAdapterInfo()
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            
            //loop thru adapters and find the one 
            foreach (NetworkInterface n in adapters)
            {
                if (n.Name == _adapterName)
                {
                    _nic = n;
                    break;
                }
            }

            UpdateStatusLbl(String.Format($"Address Changed Detected on {_nic.Name}"));

            //Run UpdateAdapterInfo() if the adapter is Up, if not, place message in label that reports it down
            if (_nic.OperationalStatus == OperationalStatus.Up)
            {
                try
                {
                    UpdateAdapterInfo();
                }
                catch (Exception ex)
                {
                    Console.Write("Error in NetworkAvailabilityChangedCallback: {0}", ex.Message);
                }
            }
            else
            {
                UpdateStatusLbl(String.Format($"{_adapterName} is down..."));
                SetIpaText(String.Empty);
                SetSpeed();
            }
        }

        private void CheckAdapterAfterTimeOut(object sender, EventArgs e)
        {
            //upon expiration of timer, Rerun UpdateAdapterInfo()
            _adapterCheckTimer.Stop();
            UpdateAdapterInfo();
        }

        public void ProcessRequest(Process p)
        {
            try
            {
                _ = p.Start();
                _ = p.WaitForExit(10000);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in ProcessRequest", ex);
                UpdateStatusLbl(String.Format($"Error Processing Request:{ex.Message}\n"));
            }
        }

        public void GetAdapterInfoAtStartup()
        {
            try
            {
                UpdateStatusLbl("Waiting for active adapter");
                SetIpaText(String.Empty);

                //grab list of all NetworkInterfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                //loop through list and find interface that is both "Up" and contains the word 'Ethernet' in it
                foreach (NetworkInterface nic in interfaces)
                {
                    IPInterfaceProperties adapterProps = nic.GetIPProperties();
                    IPv4InterfaceProperties ipv4Props = adapterProps.GetIPv4Properties();

                    if (nic.OperationalStatus == OperationalStatus.Up & nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        _adapterType = nic.NetworkInterfaceType;

                        if (_adapterType == NetworkInterfaceType.Wireless80211) //selected adapter 
                        {
                            if (nic.Name.Contains("Wi-Fi"))
                            {
                                SendAdapterUpdateToUI(nic, ipv4Props, false);
                            }

                        }
                        else if (_adapterType == NetworkInterfaceType.Ethernet) //selected adapter 
                        {
                            if (nic.Name.Contains("Ethernet"))
                            {
                                SendAdapterUpdateToUI(nic, ipv4Props, true);
                            }
                            else
                            {
                                UpdateStatusLbl("No Ethernet Adapter Found. Adapter must contain 'Ethernet' in its name.");
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Logger.LogError("Error in GetAdapterInfoAtStartup", ex);
                Console.WriteLine(ex.ToString());
                UpdateStatusLbl("Error in UpdateAdapterInfo");
            }

            if (_adapterCount == 0)
            {
                //if no active wired adapters found, start a 5 second timer
                //after 5 seconds, run UpdateAdapterInfo() again
                _adapterCheckTimer = new System.Windows.Forms.Timer();
                _adapterCheckTimer.Tick += new EventHandler(CheckAdapterAfterTimeOut);
                _adapterCheckTimer.Interval = 5000;
                _adapterCheckTimer.Start();
                UpdateStatusLbl("Waiting for active adapter");
            }
            
        }

        public void UpdateAdapterInfo()
        {
            try
            {
                UpdateStatusLbl("Waiting for active adapter");
                SetIpaText(String.Empty);

                //grab list of all NetworkInterfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                //loop through list and find interface that is both "Up" and contains the word 'Ethernet' in it
                foreach (NetworkInterface nic in interfaces)
                {

                    IPInterfaceProperties adapterProps = nic.GetIPProperties();
                    IPv4InterfaceProperties ipv4Props = adapterProps.GetIPv4Properties();

                    if (nic.OperationalStatus == OperationalStatus.Up & nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        if (_adapterType == NetworkInterfaceType.Wireless80211) //selected adapter 
                        {
                            if (nic.Name.Contains("Wi-Fi"))
                            {
                                SendAdapterUpdateToUI(nic,ipv4Props,false);
                            }

                        }
                        else if (_adapterType == NetworkInterfaceType.Ethernet) //selected adapter 
                        {
                            if (nic.Name.Contains("Ethernet"))
                            {
                                SendAdapterUpdateToUI(nic, ipv4Props, true);
                            }                           
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Logger.LogError("Error in UpdateAdapterInfo", ex);
                Console.WriteLine(ex.ToString());
                UpdateStatusLbl("Error in UpdateAdapterInfo");
            }

            if (_adapterCount == 0)
            {
                //if no active wired adapters found, start a 5 second timer
                //after 5 seconds, run UpdateAdapterInfo() again
                _adapterCheckTimer = new System.Windows.Forms.Timer();
                _adapterCheckTimer.Tick += new EventHandler(CheckAdapterAfterTimeOut);
                _adapterCheckTimer.Interval = 5000;
                _adapterCheckTimer.Start();
                UpdateStatusLbl("Waiting for active adapter");
            }
        }

        private void UpdateAdapterUIInfo(UnicastIPAddressInformation ip)
        {
            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                SetSpeed();
 
                this.Dispatcher.Invoke(() => 
                {
                    adapterTxt.Content = String.Format($"{_adapterName}");
                    hostnameTxt.Content = Dns.GetHostName();     
                    domainTxt.Content = GetDomainName();
                    SetIpaText(String.Format($"{ip.Address} / {ip.PrefixLength}"));
                });
      

                if (ip.Address.ToString() == _choice2Address)
                {
                    choiceSelect = 2;
                    ChoiceInterlock(choiceSelect);
                }
                else if (ip.Address.ToString() == _choice3Address)
                {
                    choiceSelect = 3;
                    ChoiceInterlock(choiceSelect);
                }
                else if (ip.Address.ToString() == _choice4Address)
                {
                    choiceSelect = 4;
                    ChoiceInterlock(choiceSelect);
                }
            }
        }

        public Process CreateProcess(string adapter, string ipMaskString)
        {
            try
            {
                var p = new Process();
               
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = $"interface ipv4 set address name=\"{adapter}\" {ipMaskString}\"",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    Verb = "runas",
                    RedirectStandardOutput = false,
                };
                
                p.StartInfo = info;

                return p;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in CreateProcess: {ex}");
                UpdateStatusLbl("Error Creating Process");
            }

            return null;
        }

        public string GetDomainName()
        {
            try
            {
                //Domain domain = Domain.GetCurrentDomain();
                string domainName = Environment.UserDomainName;
                if (debug) { Console.WriteLine($"Domain Name : {domainName}"); }
                return domainName;
            }
            catch (Exception)
            {
                Console.WriteLine("Could not retieve Domain Name");
                return "Not Defined";
            }
        }

        /// <summary>
        /// Manages upadting the UI with info concerning the Adapter Name, IP, Domain etc
        /// </summary>
        /// <param name="nic" >The Network Interface to be used</param>
        /// <param name="ipv4Props">The IPv4 Properties object for the Nic</param>
        /// <param name="isEthernet">Wether or not the NIC is wireless or wired ethernet</param>
        public void SendAdapterUpdateToUI(NetworkInterface nic, IPv4InterfaceProperties ipv4Props, bool isEthernet)
        {     
            if (!nic.Name.ToLower().Contains("vethernet") & !nic.Name.ToLower().Contains("loopback") & !nic.Name.ToLower().Contains("bluetooth") & !nic.Description.ToLower().Contains("virtual"))
            {
                //once a valid adapter is found, places the name in the adapterName box and sets the adapter variable used in the processes to the name
                _nic = nic;
                _adapterName = nic.Name;

                IPInterfaceProperties prop = _nic.GetIPProperties();

                //domain name display 
                string domainName = GetDomainName();
                //string domainNameForUI = string.IsNullOrEmpty(Environment.UserDomainName) ? "none defined" : Environment.UserDomainName;

                //speed display
                _speed = SpeedCalc(nic);
                SetSpeed();

                //update UI fields
                this.Dispatcher.Invoke(() =>
                {
                    adapterTxt.Content = String.Format($"{_adapterName}");
                    domainTxt.Content = domainName;
                    UpdateStatusLbl(String.Empty);
                });

                isEthernetSelected = isEthernet;
                isWifiSelected = !isEthernet;

                UdpateInterfaceButton();

                if (ipv4Props.IsDhcpEnabled)
                {
                    choiceSelect = 1;
                    ChoiceInterlock(choiceSelect);
                }

                _adapterCount++;

                foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                {
                    UpdateAdapterUIInfo(ip);
                }

                if (_adapterCount != 0)
                {
                    return;
                }
            }
        }
        #endregion

        
        #region ******** UI Processing ********
        private void UpdateStatusLbl(string msg)
        {
            
            if (!msg.Equals(String.Empty))
            {
                this.Dispatcher.Invoke(() =>
                {
                    SetVisibilityForLabel(statusLbl, true);
                    statusTxt.Content = String.Format($"{msg}");

                });
            }
            else
            {
                this.Dispatcher.Invoke(() => 
                {
                    SetVisibilityForLabel(statusLbl, false);
                    statusTxt.Content = String.Empty;
                });
            }
        }

        private void UdpateInterfaceButton()
        {
            this.Dispatcher.Invoke(() => 
            {
                wifiBtn.IsChecked = isWifiSelected;
                ethernetBtn.IsChecked = isEthernetSelected;
            });
        }

        private void ResetUserEntryText()
        {
            this.Dispatcher.Invoke(() => 
            {
                userEntryTxt.Text = String.Empty;
                userEntryApplyBtn.IsEnabled = false;
            });

        }

        private void OnKeyDownInUserEntryBoxHandler(object sender, KeyEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    System.Windows.Forms.Application.Exit();
                }          
            }));
        }

        private void userEntryApplyBtn_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"UserEntrTxt: {userEntryTxt.Text}");

            if (choiceSelect == 5)
            {

                //split character
                string sep = @" ";
                bool validIP;
                bool validMask;

                try
                {
                    if (userEntryTxt.Text.Contains(" "))
                    {
                        //split user input string into ipa and ipm
                        string[] customIP = userEntryTxt.Text.Split(sep.ToCharArray());


                        validIP = IPAddress.TryParse(customIP[0], out IPAddress ip);
                        validMask = IPAddress.TryParse(customIP[1], out IPAddress mask);


                        if (!validIP || !validMask)
                        {
                            UpdateStatusLbl("Not a Valid IP Address! Try Again");
                        }
                        else
                        {
                            Process p = new Process();

                            ProcessStartInfo info = new ProcessStartInfo
                            {
                                FileName = "netsh.exe",
                                Arguments = String.Format("interface ipv4 set address name=\"{0}\" static {1} {2}", _adapterName, ip, mask),
                                CreateNoWindow = true,
                                UseShellExecute = true,
                                Verb = "runas",
                                RedirectStandardOutput = false,
                            };
                            p.StartInfo = info;
                            ProcessRequest(p);
                            UpdateAdapterInfo();
                        }
                    }
                    else if (userEntryTxt.Text.Contains("/"))
                    {
                        sep = @"/";
                        string[] customIP = userEntryTxt.Text.Split(sep.ToCharArray());

                        validIP = IPAddress.TryParse(customIP[0], out IPAddress ip);
                        bool validMaskBits = int.TryParse(customIP[1], out int maskBits);

                        if (validMaskBits)
                        {
                            string mask = null;

                            switch (maskBits)
                            {
                                case 8:
                                    mask = "255.0.0.0";
                                    break;
                                case 16:
                                    mask = "255.255.0.0";
                                    break;
                                case 22:
                                    mask = "255.255.252.0";
                                    break;
                                case 23:
                                    mask = "255.255.254.0";
                                    break;
                                case 24:
                                    mask = "255.255.255.0";
                                    break;

                                default:
                                    mask = null;
                                    UpdateStatusLbl("Invalid Maskbits! This app only supports '/8', '/16', '/22', '/23', or '/24'");
                                    break;
                            }

                            if (!string.IsNullOrEmpty(mask))
                            {
                                Process p = new Process();

                                ProcessStartInfo info = new ProcessStartInfo
                                {
                                    FileName = "netsh.exe",
                                    Arguments = String.Format("interface ipv4 set address name=\"{0}\" static {1} {2}", _adapterName, ip, mask),
                                    CreateNoWindow = true,
                                    UseShellExecute = true,
                                    Verb = "runas",
                                    RedirectStandardOutput = false,
                                };

                                p.StartInfo = info;
                                ProcessRequest(p);
                                UpdateAdapterInfo();
                            }
                        }
                        else
                        {
                            UpdateStatusLbl("Invalid Maskbits! This app only supports '/8', '/16', '/22', '/23', or '/24'");
                        }
                    }
                    else
                    {
                        UpdateStatusLbl("Invalid Entry! Try Again. (OnKeyDown)");
                    }
                }

                catch (IndexOutOfRangeException)
                {
                    UpdateStatusLbl("You must enter an IPAddress followed by a single space, then a Subnet Mask");
                }
                catch (Exception)
                {
                    UpdateStatusLbl("Invalid! Try Again");
                }
            }
        }

        private void OnUserEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            // Handle text changed events
            userEntryApplyBtn.IsEnabled = true;

            var textBox = sender as TextBox;
            if (textBox != null)
            {
                Console.WriteLine($"Text changed to: {textBox.Text}");
            }
        }

        private void OnKeyDownInMainWindowHandler(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"KeyDownMainWindow: {e.Key}");

            if (e.Key == Key.Escape)
            {
                System.Windows.Application.Current.Shutdown();   
            }
        }

        private void ipaTxt_TextChanged(object sender, EventArgs e)
        {
            if ((string)ipaTxt.Content == String.Empty)
            {
                this.Dispatcher.Invoke(() => 
                {
                    ipaTxt.Content = "awaiting adapter...";
                });
                
            }
        }

        private void SetIpaText(string text)
        {
            this.Dispatcher.Invoke(() => 
            {
                ipaTxt.Content = text;
            });
            
        }

        private long SpeedCalc(NetworkInterface nic)
        {
            return nic.Speed;
        }

        private void SetSpeed()
        { 
            string speedSuffix = string.Empty;
            decimal div = 0;

            if (Math.Round(_speed  / 1000000d, 0) >= 999) 
            {
                speedSuffix = "Gbps";
                div = 1000;
            }
            else
            {
                speedSuffix = "Mbps";
                div = 1;
            }

            this.Dispatcher.Invoke(() =>
            {
                speedTxt.Content = String.Format($"{Math.Round(_speed  / (1000000 * div), 0)} {speedSuffix}");
            });


        }

        public void SetVisibilityForLabel(Label label, bool isVisible)
        {
            if (isVisible)
            {
                this.Dispatcher.Invoke(() =>
                {
                    label.Visibility = Visibility.Visible;
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    label.Visibility = Visibility.Hidden;
                });
            }
        }
        private void FadeInAndOut(DependencyObject d)
        {
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(3)
            };

            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(2)
            };

            // Create a Storyboard to sequence the animations
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(fadeInAnimation);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(fadeInAnimation, d);

            // Begin the fade-in
            storyboard.Begin();

            // After the fade-in completes, start the fade-out
            storyboard.Completed += (s, e) =>
            {
                Storyboard storyboardOut = new Storyboard();
                storyboardOut.Children.Add(fadeOutAnimation);
                Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
                Storyboard.SetTarget(fadeOutAnimation, d);
                storyboardOut.Begin();
            };
        }
        #endregion

    }
}
