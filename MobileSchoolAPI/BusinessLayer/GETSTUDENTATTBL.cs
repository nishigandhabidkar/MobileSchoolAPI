﻿using MobileSchoolAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileSchoolAPI.BusinessLayer
{
    public class GETSTUDENTATTBL
    {
        SchoolContext db = new SchoolContext();
        public object GETSTUDENT (GETSTUDENTPARAM OBJ)
        {
            try
            {

                var getstudent = db.VIEWGETSTUDENTATTs.Where(r => r.STANDARDID == OBJ.STANDARDID && r.DIVISIONID == OBJ.DIVISIONID).ToList();

                if (getstudent == null)
                    return new Error() { IsError = true, Message = "STUDENT NOT FOUND." };
                else
                    return getstudent;


                //var attendance = db.VIEWATTENDANCEs.
                //                    Where(r => r.ATTEDANCEDATE == DATE && r.DISPLAY == 1 )
                //                    .FirstOrDefault();
                //if (attendance == null)
                //    return new Error() { IsError = true, Message = "ATTENDANCE IS NOT AVAILABLE." };
                //else
                //    return attendance;
            }
            catch (Exception ex)
            {
                return new Error() { IsError = true, Message = ex.Message };
            }
        }
    }
}