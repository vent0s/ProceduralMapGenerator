using System.Collections;

namespace septim.core.threading
{
    public class CoroutineBody
    {
        public CoroutineBody(IEnumerator input)
        {
            this.coroutine = input;
            DataHandler.GetInstance().coroutines.Add(this, this.coroutine);
            GameManager.instance.StartCoroutine(this.coroutine);
        }

        /// <summary>
        /// define actual coroutine behavior, call deregistration when coroutine done.
        /// Do not pass any value into the method since it might make design sophisticated, 
        /// instead, using datahandler or other singleton instances to aquire necessary attributes
        /// </summary>
        /// <returns></returns>
        public IEnumerator coroutine;

        /// <summary>
        /// call this method when assining this to a thread body
        /// </summary>
        public void CoroutineRegistration()
        {

        }

        /// <summary>
        /// Call this method when this coroutine donw
        /// </summary>
        public void CoroutineDeregistration()
        {

        }
    }
}
