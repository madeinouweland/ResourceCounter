using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ResourceCounter.ViewModels
{
    public class ObservableObject : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged<TViewModel>(Expression<Func<TViewModel>> property) {
            var expression = property.Body as MemberExpression;
            if (expression != null)
            {
                var member = expression.Member;

                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs(member.Name));
                }
            }
        }
    }
}