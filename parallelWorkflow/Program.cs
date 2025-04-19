using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using Microsoft.Extensions.DependencyInjection;
using WorkflowParallelSample;

namespace WorkflowParallelSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup the service provider
            var serviceProvider = ConfigureServices();
            var host = serviceProvider.GetService<IWorkflowHost>();

            // Register the workflow definition
            host.RegisterWorkflow<ParallelWorkflow>();

            // Start the workflow engine
            host.Start();

            // Start a new instance
            await host.StartWorkflow("parallel-sample");

            Console.WriteLine("Workflow started. Press any key to exit...");
            Console.ReadKey();

            host.Stop();
        }

        private static IServiceProvider ConfigureServices()
        {
            // Create service collection and configure our services
            IServiceCollection services = new ServiceCollection();
            services.AddLogging(); // Ensure the required namespace is included
            services.AddWorkflow();

            // Build service provider
            return services.BuildServiceProvider();
        }
    }

    // Step 1: SayHello Step
    public class SayHello : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine("Hello!");
            return ExecutionResult.Next();
        }
    }

    // Step 2: PrintMessage Step
    public class PrintMessage : StepBody
    {
        public string Message { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine(Message);
            return ExecutionResult.Next();
        }
    }

    // Step 3: SayGoodbye Step
    public class SayGoodbye : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine("Goodbye!");
            return ExecutionResult.Next();
        }
    }

    // Workflow Definition
    public class ParallelWorkflow : IWorkflow
    {
        public string Id => "parallel-sample";
        public int Version => 1;

        public void Build(IWorkflowBuilder<object> builder)
        {
            builder
                .StartWith<SayHello>()
                .Parallel()
                    .Do(then =>
                        then.StartWith<PrintMessage>()
                                .Input(step => step.Message, data => "Item 1.1")
                            .Then<PrintMessage>()
                                .Input(step => step.Message, data => "Item 1.2"))
                    .Do(then =>
                        then.StartWith<PrintMessage>()
                                .Input(step => step.Message, data => "Item 2.1")
                            .Then<PrintMessage>()
                                .Input(step => step.Message, data => "Item 2.2"))
                    .Do(then =>
                        then.StartWith<PrintMessage>()
                                .Input(step => step.Message, data => "Item 3.1")
                            .Then<PrintMessage>()
                                .Input(step => step.Message, data => "Item 3.2"))
                .Join()
                .Then<SayGoodbye>();
        }
    }
}

//SayHello
//   |
//Parallel
// / | \
//1  2  3
// \ | /
// Join
//  |
//SayGoodbye


//using System;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using WorkflowCore.Interface;
//using WorkflowCore.Models;

//namespace ConditionalWorkflowDemo
//{
//    class Program
//    {
//        static async Task Main(string[] args)
//        {
//            var serviceProvider = ConfigureServices();
//            var host = serviceProvider.GetService<IWorkflowHost>();

//            host.RegisterWorkflow<ConditionalWorkflow, MyData>();

//            host.Start();

//            // Run workflow for true
//            Console.WriteLine("Running workflow with IsApproved = true");
//            await host.StartWorkflow("conditional-workflow", new MyData { IsApproved = true, StatusCode = 1 });

//            // Run workflow for false
//            Console.WriteLine("Running workflow with IsApproved = false");
//            await host.StartWorkflow("conditional-workflow", new MyData { IsApproved = false, StatusCode = 0 });

//            // Run workflow with a different status code
//            Console.WriteLine("Running workflow with StatusCode = 2");
//            await host.StartWorkflow("conditional-workflow", new MyData { IsApproved = false, StatusCode = 2 });

//            Console.ReadKey();
//            host.Stop();
//        }

//        private static IServiceProvider ConfigureServices()
//        {
//            var services = new ServiceCollection();
//            services.AddLogging();
//            services.AddWorkflow();
//            return services.BuildServiceProvider();
//        }
//    }

//    public class MyData
//    {
//        public bool IsApproved { get; set; }
//        public int StatusCode { get; set; }
//    }

//    public class CheckUserStatus : StepBody
//    {
//        public override ExecutionResult Run(IStepExecutionContext context)
//        {
//            Console.WriteLine("Checking user status...");
//            return ExecutionResult.Next();
//        }
//    }

//    public class SendWelcomeEmail : StepBody
//    {
//        public override ExecutionResult Run(IStepExecutionContext context)
//        {
//            Console.WriteLine("User approved ✅: Sending welcome email.");
//            return ExecutionResult.Next();
//        }
//    }

//    public class SendRejectionEmail : StepBody
//    {
//        public override ExecutionResult Run(IStepExecutionContext context)
//        {
//            Console.WriteLine("User rejected ❌: Sending rejection email.");
//            return ExecutionResult.Next();
//        }
//    }

//    public class SendPendingNotification : StepBody
//    {
//        public override ExecutionResult Run(IStepExecutionContext context)
//        {
//            Console.WriteLine("User status is pending ⏳: Sending notification.");
//            return ExecutionResult.Next();
//        }
//    }

//    public class SendInvalidStatusNotification : StepBody
//    {
//        public override ExecutionResult Run(IStepExecutionContext context)
//        {
//            Console.WriteLine("Unknown status ❓: Sending alert to admin.");
//            return ExecutionResult.Next();
//        }
//    }

//    public class ConditionalWorkflow : IWorkflow<MyData>
//    {
//        public string Id => "conditional-workflow";
//        public int Version => 1;

//        public void Build(IWorkflowBuilder<MyData> builder)
//        {
//            builder
//                .StartWith<CheckUserStatus>()

//                // 1. Basic if-else condition
//                .Decide(data => data.IsApproved)
//                    .Branch(true, then => then.StartWith<SendWelcomeEmail>())
//                    .Branch(false, then =>
//                        // 2. Nested Decide like switch-case on StatusCode
//                        then.Decide(data => data.StatusCode)
//                            .Branch(0, next => next.StartWith<SendRejectionEmail>())
//                            .Branch(1, next => next.StartWith<SendPendingNotification>())
//                            .Branch(2, next => next.StartWith<SendInvalidStatusNotification>())
//                    );
//        }
//    }
//}



//+-------------------+
//| CheckUserStatus |
//+--------+----------+
//         |
//         v
//   IsApproved ?
//    /      \
//  Yes       No
//  |          |
//  v          v
//SendWelcome  SendRejection

