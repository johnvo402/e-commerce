using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Model;
using App.Models;

namespace App.Model
{
    public class MethodPayment
    {
        public AppUser users { get; set; }
        public int methods { get; set; }

        public MethodPayment(AppUser user, int method)
        {
            users = user;
            methods = method;
        }
    }
}