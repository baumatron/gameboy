namespace GameBoyTests
{
    class InstructionTestRunner
    {
        public InstructionTestRunner(InstructionTest test)
        {
            _test = test;
        }

        public void Run()
        {
            _test.PrepareTest();

            _test.ValidatePreExecute();

            _test.ExecuteTestSubject();

            _test.ValidatePostExecute();
        }

        InstructionTest _test;
    }
}