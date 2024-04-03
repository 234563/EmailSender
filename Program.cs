using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading.Tasks;


namespace EmailSenderProgram
{
    internal class Program
    {
        #region Properties
        private readonly CustomerRepository _customerRepository;
        public IContainer Container;
        #endregion

        #region Constructor
        public Program()
        {
            Container = Configure();

            _customerRepository = Container.Resolve<CustomerRepository>();
        }
        #endregion



        /// <summary>
        /// This application is run everyday
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Console.WriteLine("Send Welcomemail");

            Program program = new Program();
            
            Task.Run(async () =>
            {
                await program.RunProgram();
            }).Wait();


            Console.ReadKey();
        }

        #region Methods

        private static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new SmtpClient("smtp.gmail.com")).As<SmtpClient>();

            builder.RegisterType<EmailSender>().As<IEmailSender>();

            DataLayer datacontext = new DataLayer();
            builder.RegisterInstance(datacontext).AsSelf();

            builder.RegisterType<FileLogger>().As<ILogger>();

            builder.RegisterType<CustomerRepository>();


            var container = builder.Build();

            return container;
        }

        private async Task RunProgram()
        {
            // using await if second one is dependent on first one else not required
            bool success = await _customerRepository.CustomerMailSender(MessageTypeEnum.NewUser, "EOComebackToUs");

            if (!success)
                Console.WriteLine("Something went wrong at with new customer email sendr .");

            success = await _customerRepository.CustomerMailSender(MessageTypeEnum.ReturningUser, "EOComebackToUs");

            if (!success)
                Console.WriteLine("Something went wrong at retuning customer email sender ");
        }

        #endregion


    }
}