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



//builder
//    .StartWith<CheckUserStatus>()
//    .Decide(data => ((MyData)data).IsApproved)
//        .Branch(true, then => then.StartWith<SendWelcomeEmail>())
//        .Branch(false, then => then.StartWith<SendRejectionEmail>());

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

