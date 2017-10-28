public class FunctionChain<T>
    {
        private FunctionChainElement<T> FunctionChainHead { get; set; }
        private FunctionChainElement<T> FunctionChainTail { get; set; }
        private FunctionChainElement<T> CurrentlyExecutedFunctionChainElement { get; set; }
 
        public void AddFunction(IFunctionChainElement<T> functionChainElement)
        {
            AddFunction(functionChainElement.ForwardFunction, functionChainElement.ReverseFunction);
        }
        public void AddFunction(Func<FunctionChainMessage<T>, Task<FunctionChainMessage<T>>> forwardFunc,
            Func<FunctionChainMessage<T>, Task<FunctionChainMessage<T>>> reverseFunc)
        {
            if (FunctionChainTail == null)
            {
                FunctionChainTail = new FunctionChain<T>.FunctionChainElement<T>(forwardFunc, reverseFunc);
                FunctionChainHead = FunctionChainTail;
            }
            else
            {
                FunctionChainHead = new FunctionChain<T>.FunctionChainElement<T>(forwardFunc, reverseFunc, FunctionChainHead);
            }
        }
 
        public async Task<FunctionChainMessage<T>> ExecuteChain(T argument)
        {
            FunctionChainMessage<T> chainMessage = new FunctionChainMessage<T>(argument);
            FunctionChainElement<T> currentFunction = FunctionChainTail;
            await SuccessfulExecutionLoop(chainMessage);
            return chainMessage;
        }
 
        private async Task<FunctionChainMessage<T>> SuccessfulExecutionLoop(FunctionChainMessage<T> chainMessage)
        {
 
            CurrentlyExecutedFunctionChainElement = FunctionChainTail;
            if(CurrentlyExecutedFunctionChainElement == null)
                return chainMessage;
 
            while (chainMessage.IsOk && CurrentlyExecutedFunctionChainElement != null)
            {
                try
                {
                    chainMessage = await CurrentlyExecutedFunctionChainElement.ForwardFunc(chainMessage);
                }
                catch (Exception e)
                {
                    chainMessage.Exception = e;
                    return await UnsuccessfulExecutionLoop(chainMessage);
                }
 
                if (chainMessage.IsOk)
                {
                    chainMessage.CompletedFunctions++;
                    CurrentlyExecutedFunctionChainElement = CurrentlyExecutedFunctionChainElement.Successor;
                }
                else
                    return await UnsuccessfulExecutionLoop(chainMessage);
            }
 
            return chainMessage;
        }
 
        private async Task<FunctionChainMessage<T>> UnsuccessfulExecutionLoop(FunctionChainMessage<T> chainMessage)
        {
            if (CurrentlyExecutedFunctionChainElement.HasPredecessor)
                do
                {
                    CurrentlyExecutedFunctionChainElement = CurrentlyExecutedFunctionChainElement.Predecessor;
                    chainMessage = await CurrentlyExecutedFunctionChainElement.ReverseFunc(chainMessage);
                } while (CurrentlyExecutedFunctionChainElement.HasPredecessor);
 
            return chainMessage;
        }
 
        private class FunctionChainElement<T>
        {
            public FunctionChainElement(Func<FunctionChainMessage<T>, Task<FunctionChainMessage<T>>> forwardFunc,
                Func<FunctionChainMessage<T>, Task<FunctionChainMessage<T>>> reverseFunc, FunctionChainElement<T> predecessor = null)
            {
                ForwardFunc = forwardFunc;
                ReverseFunc = reverseFunc;
                if (predecessor != null)
                {
                    predecessor.Successor = this;
                    Predecessor = predecessor;
                }
            }
            public Func<FunctionChainMessage<T>, Task<FunctionChainMessage<T>>> ForwardFunc { get; set; }
            public Func<FunctionChainMessage<T>, Task<FunctionChainMessage<T>>> ReverseFunc { get; set; }
 
            public FunctionChainElement<T> Successor { get; set; }
            public FunctionChainElement<T> Predecessor { get; set; }
 
            public bool HasSuccessor
            {
                get { return Successor != null; }
            }
            public bool HasPredecessor
            {
                get { return Predecessor != null; }
            }
        }
 
    }
 
    public interface IFunctionChainElement<T>
    {
        Task<FunctionChainMessage<T>> ForwardFunction(FunctionChainMessage<T> argument);
        Task<FunctionChainMessage<T>> ReverseFunction(FunctionChainMessage<T> argument);
    }
 
    public class FunctionChainMessage<T>
    {
        public FunctionChainMessage(T value)
        {
            Value = value;
            IsOk = true;
        }
        public T Value { get; set; }
        public bool IsOk { get; set; }
        public string Message { get; set; }
        private Exception _exception;
        public int CompletedFunctions { get; set; }
        public Exception Exception
        {
            get
            {
                return _exception;
            }
            set
            {
                _exception = value;
                IsOk = false;
            }
        }
    }