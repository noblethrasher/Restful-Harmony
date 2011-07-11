using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace nthings2.src.app
{
    public class ViewPage<T> : Page
    {
        public T Model { get; set; }

        protected override void TrackViewState()
        {
            
        }

        protected override void LoadViewState(object savedState)
        {
            
        }

        protected override void SavePageStateToPersistenceMedium(object state)
        {
            
        }
    }

    
}