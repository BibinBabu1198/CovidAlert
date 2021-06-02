using GACSWebApi.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace InfrasysDataImporter
{
    public partial class DataImporter : Form
    {

        #region Private Variables

        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        bool exitReasonKnown = false;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem exitMenuItem;
        string appPath = AppDomain.CurrentDomain.BaseDirectory.ToString();
        int readFrequency;
        bool isBusy;
        const string appName = "Email For Covid";
        string EmailId = string.Empty;
        string SMTPName = "";
        int Port = 0;
        string UserName = string.Empty;
        string Password = string.Empty;
        string Template = string.Empty;
        string whatapp = string.Empty;
        bool Tosend = false;
        const string accountSid = "web_api_key";
        const string authToken = "token";




        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="DataImporter"/> class.
        /// </summary>
        public DataImporter()
        {
            InitializeComponent();
            ConfigUI();
            StartListentingExitEvents();
            readFrequency = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["read_frequency"].ToString());
            EmailId = System.Configuration.ConfigurationManager.AppSettings["EmailIds"].ToString();
            SMTPName = System.Configuration.ConfigurationManager.AppSettings["SMTPNAME"].ToString();
            Port = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["Port"].ToString());
            UserName = System.Configuration.ConfigurationManager.AppSettings["Username"].ToString();
            Password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();
            Template = System.Configuration.ConfigurationManager.AppSettings["Template"].ToString();
            whatapp = System.Configuration.ConfigurationManager.AppSettings["whatapp"].ToString();
        }
        #endregion

        #region Methods

        #region ConfigUI

        private void ConfigUI()
        {
            Logger.Info("//////////////////////////////////////////////////////////////////////////////////");
            Logger.Info(appName + " Started");
            Logger.Info("//////////////////////////////////////////////////////////////////////////////////");

            try
            {

                //Context Menu
                this.contextMenu = new System.Windows.Forms.ContextMenu();
                this.exitMenuItem = new System.Windows.Forms.MenuItem();
                this.contextMenu.MenuItems.AddRange(
                    new System.Windows.Forms.MenuItem[] { this.exitMenuItem });
                this.exitMenuItem.Index = 0;
                this.exitMenuItem.Text = "Exit";
                this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);

                this.components = new System.ComponentModel.Container();
                this.niIcon = new System.Windows.Forms.NotifyIcon(this.components);
                this.ShowInTaskbar = false;
                niIcon.Text = appName;
                niIcon.Visible = true;
                niIcon.BalloonTipTitle = appName;
                niIcon.BalloonTipText = "is running...";
                niIcon.ContextMenu = contextMenu;
                niIcon.Icon = new Icon(appPath + "\\Assets\\Icons\\Write.ico");
                //Keep the Baloon for 2 second
                niIcon.ShowBalloonTip(2000);
                WindowState = FormWindowState.Minimized;
                niIcon.BalloonTipClosed += (sender, e) =>
                {
                    var thisIcon = (NotifyIcon)sender;
                    thisIcon.Visible = false;
                    thisIcon.Dispose();
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error in configuring UI : " + ex.Message);
            }

        }
        #endregion

        #region StartListentingExitEvents

        /// <summary>
        /// Starts  listenting exit events.
        /// </summary>
        private void StartListentingExitEvents()
        {
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SessionEnded);
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.FormClosing += RMDataImporter_FormClosing;
        }
        #endregion

        public void Email(string htmlString)
        {
            try
            {
                string[] pares = EmailId.Split(',');
                foreach (string Emailid in pares)
                {
                    MailMessage message = new MailMessage();
                    SmtpClient smtp = new SmtpClient();
                    message.From = new MailAddress(UserName);
                    message.To.Add(new MailAddress(Emailid));
                    message.Subject = "Covid Vaccine Alert";
                    message.IsBodyHtml = true; //to make message body as html  
                    message.Body = htmlString;
                    smtp.Port = Port;
                    smtp.Host = SMTPName; //for gmail host  
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential(UserName, Password);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #region Events

        #region Application Exit events


        #region Main_FormClosing
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Logger.Info("//////////////////////////////////////////////////////////////////////////////////");
                Logger.Info("Email for covid Interface stopped");
                Logger.Info("//////////////////////////////////////////////////////////////////////////////////");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        #endregion

        #region exitMenuItem_Click
        /// <summary>
        /// Handles the Click event of the exitMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Info("App terminated from context menu");
            Application.Exit();
        }
        #endregion

        #region SessionEnded
        /// <summary>
        /// Sessions  ended.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SessionEndingEventArgs"/> instance containing the event data.</param>

        private void SessionEnded(object sender, SessionEndingEventArgs e)
        {
            Logger.Info("::::::::::::::::::::::::APPLICATION TERMINATED::::::::::::::::::::::::::::::::");
            switch (e.Reason)
            {
                case Microsoft.Win32.SessionEndReasons.Logoff:
                    MessageBox.Show("User Loggerging off");
                    exitReasonKnown = true;
                    break;

                case Microsoft.Win32.SessionEndReasons.SystemShutdown:
                    MessageBox.Show("Computer is shutting down");
                    exitReasonKnown = true;
                    break;
            }
            if (!exitReasonKnown)
            {
                Logger.Info("App termination due to unknown reason");
            }
            exitReasonKnown = false;
        }
        #endregion

        #region OnApplicationExit
        /// <summary>
        /// Called when [application exit].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnApplicationExit(object sender, EventArgs e)
        {
            Logger.Info("::::::::::::::::::::::::APPLICATION TERMINATED::::::::::::::::::::::::::::::::");
        }
        #endregion


        #region CurrentDomain_UnhandledException

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Info("::::::::::::::::::::::::CATASTRPOHIC FAILURE::::::::::::::::::::::::");
            Logger.Error(e.ExceptionObject);
        }
        #endregion

        #region RMDataImporter_FormClosing
        /// <summary>
        /// Handles the FormClosing event of the RMDataImporter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        private void RMDataImporter_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.Info("Application forcefully terminated by user");
        }
        #endregion


        #endregion

        #region Main_Load
        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                Timer _timer = new Timer();
                _timer.Interval = readFrequency;
                _timer.Tick += new EventHandler(delegate (object s, EventArgs a)
                {
                    try
                    {

                        if (!isBusy)
                        {
                            RootObject rootObject = GetInfo();
                            if (rootObject.Centers.Count > 0)
                            {
                                if(!Tosend)
                                {
                                    foreach (var item in rootObject.Centers)
                                    {
                                        foreach (var sessions in item.sessions)
                                        {
                                            if (sessions.available_capacity > 0 && sessions.available_capacity_dose1 > 0 &&( item.pincode==683546 || item.name.Equals("Apollo Adlux Hospital")))
                                            {
                                                string htmlTemplate = File.ReadAllText(Template);
                                                var replaceMents = new Dictionary<string, string> {
                                                            { TemplateMaker.Centername,item.name },
                                                            { TemplateMaker.Address,item.address},
                                                            { TemplateMaker.Pincode,item.pincode.ToString()},
                                                            { TemplateMaker.Date,sessions.date},
                                                            { TemplateMaker.AgeLimit,sessions.min_age_limit.ToString()},
                                                            { TemplateMaker.vaccine,sessions.vaccine},
                                                            { TemplateMaker.AvailableCapacity,sessions.available_capacity.ToString()},
                                                            { TemplateMaker.AvailableCapacityDose1,sessions.available_capacity_dose1.ToString()},
                                                            { TemplateMaker.AvailableCapacityDose2,sessions.available_capacity_dose2.ToString()},
                                                            { TemplateMaker.VaccineType,item.fee_type},
                                                            };
                                                 Email(TemplateMaker.GenrateTemplate(htmlTemplate, replaceMents));
                                                TwilioClient.Init(accountSid, authToken);
                                                var message = MessageResource.Create(
                                                               from: new PhoneNumber("whatsapp:number"),
                                                               to: new PhoneNumber("whatsapp:number"),
                                                               body: TemplateMaker.GenrateTemplate(File.ReadAllText(whatapp), replaceMents)
                                                           );

                                            }
                                        }
                                     
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exi)
                    {
                        isBusy = false;
                        Logger.Error(exi);

                    }
                });

                // Start the timer
                _timer.Start();

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show("Error : " + ex);
            }

        }
        #endregion

        #endregion

        #endregion

        public RootObject GetInfo()
        {

            // Initialization.  

            RootObject rootObject = new RootObject();

            //string url = "https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByPin?pincode=683546&date="+DateTime.Now.ToString("dd/MM/yyyy");
            string url= "https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByDistrict?district_id=307&date=" + DateTime.Now.ToString("dd/MM/yyyy");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "GET";


            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {

                using (Stream responseStream = response.GetResponseStream())
                {
                    // Get a reader capable of reading the response stream
                    using (StreamReader myStreamReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        // Read stream content as string
                        string responseJSON = myStreamReader.ReadToEnd();

                        // Assuming the response is in JSON format, deserialize it
                        // creating an instance of TData type (generic type declared before).
                        rootObject = JsonConvert.DeserializeObject<RootObject>(responseJSON);
                    }
                }

            }
            return rootObject;
        }


        public class Center
        {
            [JsonProperty("center_id")]
            public int center_id { get; set; }

            [JsonProperty("name")]
            public string name { get; set; }

            [JsonProperty("address")]
            public string address { get; set; }

            [JsonProperty("state_name")]
            public string state_name { get; set; }

            [JsonProperty("district_name")]
            public string district_name { get; set; }

            [JsonProperty("pincode")]
            public int pincode { get; set; }

            [JsonProperty("lat")]
            public int lat { get; set; }

            [JsonProperty("long")]
            public int longs { get; set; }

            [JsonProperty("from")]
            public string from { get; set; }

            [JsonProperty("to")]
            public string to { get; set; }

            [JsonProperty("fee_type")]
            public string fee_type { get; set; }

            [JsonProperty("sessions")]
            public List<sessions> sessions { get; set; }

            [JsonProperty("vaccine_fees")]
            public List<vaccinefees> vaccine_fees { get; set; }

          
        }

        public class sessions
        {
            [JsonProperty("session_id")]
            public string session_id { get; set; }

            [JsonProperty("date")]
            public string date { get; set; }

            [JsonProperty("available_capacity")]
            public int available_capacity { get; set; }

            [JsonProperty("min_age_limit")]
            public int min_age_limit { get; set; }

            [JsonProperty("vaccine")]
            public string vaccine { get; set; }

            [JsonProperty("slots")]
            public List<string> slots { get; set; }

            [JsonProperty("available_capacity_dose1")]
            public int available_capacity_dose1 { get; set; }

            [JsonProperty("available_capacity_dose2")]
            public int available_capacity_dose2 { get; set; }
        }

        public class vaccinefees
        {
            [JsonProperty("vaccine")]
            public string vaccine { get; set; }

            [JsonProperty("fee")]
            public int fee { get; set; }
        }

        public class RootObject
        {
            [JsonProperty("centers")]
            public List<Center> Centers { get; set; }
        }
    }
}
