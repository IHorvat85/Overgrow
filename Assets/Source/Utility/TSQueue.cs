using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility {
    public class TSQueue<T> : Queue<T> {
        // Thread-Safe Queue Implementation

        Queue<T> internalQueue;
        public TSQueue () {
            internalQueue = new Queue<T>();
        }

        public new void Enqueue (T item) {
            lock (internalQueue) {
                internalQueue.Enqueue(item);
            }
        }

        public new T Dequeue () {
            T item;
            lock (internalQueue) {
                item = internalQueue.Dequeue();
            }
            return item;
        }

        public new T Peek () {
            T item;
            lock (internalQueue) {
                item = internalQueue.Peek();
            }
            return item;
        }

        public new int Count () {
            int cnt;
            lock (internalQueue) {
                cnt = internalQueue.Count;
            }
            return cnt;
        }

    }
}
