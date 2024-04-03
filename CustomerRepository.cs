using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram
{
    public enum MessageTypeEnum
    {
        NewUser,
        ReturningUser ,
        PasswordChange,
        NewOrder,
        OrderStatus,
    }
    public class CustomerRepository
    {
        #region Properties
        private DataLayer DataContext;
        public IEmailSender _EmailSender;
        private ILogger Logger;
        #endregion

        #region Constructor
        public CustomerRepository(DataLayer _Datacontext, IEmailSender _emailSender , ILogger _logger)
        {
            DataContext = _Datacontext;
            _EmailSender = _emailSender;
            Logger = _logger;

        }
        #endregion

        #region Methods
        public async Task<bool> CustomerMailSender(MessageTypeEnum UserType , string vouchorCode)
        {
            try
            {
                int customerCount = DataContext.ListCustomers().Count();

                int batchSize = 2;
                int skip = 0;
                //using paggination if there is alot of data for better performance
                for (int i = 0; i < customerCount; i += batchSize)
                {

                    var cutonmers = DataContext.ListCustomers()
                        .Skip(skip)
                        .Take(batchSize);
                    //Also we can send it in background Parallel using Thread
                    foreach (var customer in cutonmers)
                    {
                        /// For New Customer 
                        if(customer.CreatedDateTime > DateTime.Now.AddDays(-1) && UserType == MessageTypeEnum.NewUser)
                        {
                            await SendEmailToCustomer(customer, UserType, vouchorCode);
                        }
                        //If User have any orders and returning 
                        else if (DataContext.ListOrders().Any(s => s.CustomerEmail == customer.Email))
                        {
                            continue;
                        }
                        else if (UserType == MessageTypeEnum.ReturningUser)
                        {
                            await SendEmailToCustomer(customer, UserType, vouchorCode);
                        }
                    }

                    skip += batchSize;
                }

                return true;
            }
            catch (Exception ex)
            {
                string message = "CustomerMailSender Exception: "+ex.Message +",DateAndTime:"+DateTime.Now;
                Console.WriteLine(message);
                return false;
            }

        }
        /// <summary>
        /// THis method is used to send general email to Customer 
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="_MessageType"></param>
        /// <param name="vouchorCode"></param>
        /// <returns></returns>
        public async Task<bool> SendEmailToCustomer(Customer customer, MessageTypeEnum _MessageType, string vouchorCode = "")
        {
            try
            {
                await FormatEmail(customer, MessageTypeEnum.NewUser, vouchorCode);

            }
            catch (Exception ex)
            {

                string message = "SendEmailToCustomer Exception: " + ex.Message + ",DateAndTime:" + DateTime.Now;
                Console.WriteLine(message);
                Logger.Log(message);
                return false;
            }
            return true; 
        }
        #endregion

        #region Email Helper Method
        private  async Task SendEmail(string email, string Subject, string Body)
        {
            try
            {
                //Create a new MailMessage
                System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();

                //Add email to reciever list
                mailMessage.To.Add(email);
                //Add subject
                mailMessage.Subject = Subject;
                //Send mail from info@EO.com
                mailMessage.From = new System.Net.Mail.MailAddress("infor@EO.com");
                //Add body to mail
                mailMessage.Body = Body;
                #if DEBUG
                //Don't send mails in debug mode, just write the emails in console
                Console.WriteLine("Send customer mail to:" + email);
                #else

                await _EmailSender.SendEmailAsync(mailMessage);

                #endif
                

            }
            catch (Exception ex)
            {
                
                string message = "SendEmail Exception: " + ex.Message + ",DateAndTime:" + DateTime.Now;
                Console.WriteLine(message);
                Logger.Log(message);

            }


        }
        private async Task FormatEmail(Customer customer, MessageTypeEnum _MessageType, string vouchorCode = "")
        {
            string Subject = "";
            string Body = "";
            bool HasMessageType = true;

            switch (_MessageType)
            {
                /// For New User 
                case MessageTypeEnum.NewUser:

                    Subject = "Welcome to our platform";
                    Body = "Hi " + customer.Email +
                     "<br>We miss you as a customer. Our shop is filled with nice products. Here is a voucher that gives you 50 kr to shop for." +
                     "<br>Voucher: " + vouchorCode +
                     "<br><br>Best Regards,<br>EO Team";
                    break;
                /// For New Returning User  
                case MessageTypeEnum.ReturningUser:

                    Subject = "We miss you as a customer";
                    Body = "Hi " + customer.Email +
                     "<br>We miss you as a customer. Our shop is filled with nice products. Here is a voucher that gives you 50 kr to shop for." +
                     "<br>Voucher: " + vouchorCode +
                     "<br><br>Best Regards,<br>EO Team";
                    break;
                /// For Password channge request 
                case MessageTypeEnum.PasswordChange:

                    Subject = " Password Change";
                    Body = "Hi " + customer.Email +
                     "<br>Here is your password reset Link .";
                    break;
                /// For New Order
                case MessageTypeEnum.NewOrder:

                    Subject = " Order Confirmed ";
                    Body = "Hi " + customer.Email +
                     "<br> You order is being confirmed it will be delived to you in 3 working days ";
                    break;
                /// For Order Status Check 
                case MessageTypeEnum.OrderStatus:

                    Subject = "Order Status ";
                    Body = "Hi " + customer.Email +
                     "<br> You order is being shipped it will be delived in 1 day . ";
                    break;
                default:
                    HasMessageType = false;
                    break;
            }
            // Send Email to User 
            if (HasMessageType)
            {
                await SendEmail(customer.Email, Subject, Body);
            }
        }
        #endregion

    }
}
