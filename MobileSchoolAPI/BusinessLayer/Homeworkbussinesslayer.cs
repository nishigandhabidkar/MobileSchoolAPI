﻿using MobileSchoolAPI.Models;
using MobileSchoolAPI.ResultModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;


namespace MobileSchoolAPI.BusinessLayer
{
    public class Homeworkbussinesslayer
    {
        string smsstatus = ConfigurationManager.AppSettings["SmsStatus"].ToString();
        public object Savehomework(homeworkparameters obj)
        {

            SchoolMainContext db = new ConcreateContext().GetContext(obj.Userid, obj.Password);
            if (db == null)
            {
                return new Results() { IsSuccess = false, Message = "Invalid User" };
            }
            TBLHOMEWORK objHomework = new TBLHOMEWORK();
            var getUserType = db.VW_GET_USER_TYPE.Where(r => r.UserId == obj.Userid).FirstOrDefault();




            objHomework.STANDARDID = int.Parse(obj.standardid);


            objHomework.CREATEDID = int.Parse(getUserType.EmpCode);
            objHomework.DIVISIONID = obj.division.ToString();
            objHomework.SUBJECTID = obj.subject;
            objHomework.TERMID = obj.term;
            objHomework.HOMEWORK = obj.homeworkdescription;
            objHomework.HOMEWORKDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            objHomework.TIME = DateTime.Now.ToShortTimeString();
            objHomework.DISPLAY = 1;
            objHomework.ACADEMICYEAR = "2018-2019";

            db.TBLHOMEWORKs.Add(objHomework);
            db.SaveChanges();

            TBLNOTIFICATION objnotification = new TBLNOTIFICATION();
            objnotification.TITLE = obj.homeworkdescription;
            objnotification.NOTIFICATIONDATE = DateTime.Now;
            objnotification.NOTIFICATIONTIME = DateTime.Now.ToString("h:mm tt");
            objnotification.DIVISIONID = int.Parse(obj.division);
            objnotification.ACADEMICYEAR = "2018-2019";
            objnotification.NOTIFICATIONTYPE = "Homework";
            db.TBLNOTIFICATIONs.Add(objnotification);
            db.SaveChanges();


            string[] divid = objHomework.DIVISIONID.ToString().Split(',');
            for (int d = 0; d < divid.Length; d++)
            {
                int singledivision = Convert.ToInt32(divid[d]);
                var getstudent = db.VIEWGETSTUDENTATTs.Where(r => r.DIVISIONID == singledivision).ToList();

                if (getstudent == null)
                {

                    return new Results
                    {
                        IsSuccess = false,
                        Message = "Student Not Found"
                    };



                }
                // return getstudent;
                for (int i = 0; i < getstudent.Count; i++)
                {
                    TBLNOTIFICATIONDETAIL objnotidetails = new TBLNOTIFICATIONDETAIL();
                    objnotidetails.NOTIFICATIONID = objnotification.NOTIFICATIONID;
                    objnotidetails.STUDENTID = getstudent[i].STUDENTID;
                    objnotidetails.STATUS = 0;
                    db.TBLNOTIFICATIONDETAILs.Add(objnotidetails);
                    db.SaveChanges();
                    FCMPushNotification OBJPUSH = new FCMPushNotification();
                    //var getsubjectname = db.VIEWSUBJECTNAMEs.Where(r => r.SUBJECTID == obj.subject).ToList();

                    string studentid = Convert.ToString(getstudent[i].STUDENTID);
                    var userid = db.VIEWGETUSERIDFROMEMPCODEs.Where(r => r.EmpCode == studentid).FirstOrDefault();
                    var device = db.VW_DEVICE.FirstOrDefault(r => r.UserId == userid.UserId);
                    if (device != null)
                    {
                        if (!string.IsNullOrWhiteSpace(device.DeviceId))
                            OBJPUSH.SendNotification("Homework", obj.homeworkdescription, device.DeviceId);
                    }
                    //if (smsstatus == "1")
                    //{
                    //    SMSSendTESTDLR(getstudent[i].GMOBILE, objHomework.HOMEWORK);
                    //}
                }
            }
            return new Results
            {

                IsSuccess = true,
                Message = "Homework assign successfully and SMS sent Sucessfully"
            };

        }

        public object SendNotificaiton(homeworkparameters obj)
        {
            SchoolMainContext db = new ConcreateContext().GetContext(obj.Userid, obj.Password);
            if (db == null)
            {


                return new Results() { IsSuccess = false, Message = "Invalid User" };
            }
            var device = db.VW_DEVICE.FirstOrDefault(r => r.UserId == obj.Userid);
            if (device != null)
            {
                FCMPushNotification OBJPUSH = new FCMPushNotification();
                return OBJPUSH.SendNotification("Homework", obj.homeworkdescription, device.DeviceId);
            }

            throw new Exception("Notification Failed");

        }

        public object SaveAttendance(AttendanceParameterscs atteobj)
        {
            SchoolMainContext db = new ConcreateContext().GetContext(atteobj.Userid, atteobj.Password);
            if (db == null)
            {
                return new Results() { IsSuccess = false, Message = "Invalid User" };
            }

            TBLATTENDENCEMASTER objmster = new TBLATTENDENCEMASTER();
            TBLATTENDENCE objDetail = new TBLATTENDENCE();
            var logindetail = db.TBLUSERLOGINs.
                                Where(r => r.UserId == atteobj.Userid && r.Password == atteobj.Password && r.STATUS == "ACTIVE")
                                .FirstOrDefault();

            var ClassTeacherCheck = db.TBLASSIGNCLASSTEACHERs.Where(r => r.DIVISIONID == atteobj.DIVISIONID && r.ACADEMICYEAR=="2018-2019" && r.DISPLAY==1).ToList();
            if (ClassTeacherCheck.Count != 0)
            {
                var checkatt = db.Vw_ATTENDANCECHECK.FirstOrDefault(r => r.DIVISIONID == atteobj.DIVISIONID && r.ATTEDANCEDATE == atteobj.ATTEDANCEDATE);
                //Duplicate Attendance Check
                if (checkatt == null)
                {

                    try
                    {

                        objmster.ATTEDANCEDATE = atteobj.ATTEDANCEDATE;

                        objmster.DIVISIONID = atteobj.DIVISIONID;
                        objmster.DISPLAY = 1;
                        objmster.EDUCATIONYEAR = "2018-2019";
                        var std = db.vw_FETCHSTANDARDBYDIVISION.Where(r => r.DIVISIONID == atteobj.DIVISIONID && r.DISPLAY == 1 && r.ACADEMICYEAR == "2018-2019").ToList();

                        objmster.STANDARDID = Convert.ToInt32(std[0].STANDARDID);
                        objmster.CREATEDON = DateTime.Now;

                        objmster.CREATEDID = atteobj.Userid;

                        db.TBLATTENDENCEMASTERs.Add(objmster);
                        db.SaveChanges();


                        TBLNOTIFICATION objnotification = new TBLNOTIFICATION();
                        objnotification.TITLE = "Daily Attendance";
                        objnotification.NOTIFICATIONDATE = DateTime.Now;
                        objnotification.NOTIFICATIONTIME = DateTime.Now.ToString("h:mm tt");
                        objnotification.DIVISIONID = atteobj.DIVISIONID;
                        objnotification.ACADEMICYEAR = "2018-2019";
                        objnotification.NOTIFICATIONTYPE = "Attendance";
                        db.TBLNOTIFICATIONs.Add(objnotification);
                        db.SaveChanges();





                        string absentno = atteobj.Absentno;
                        string[] sbno = absentno.Split(',');
                        objDetail.ATTEDANCEMID = objmster.ATTEDANCEMID;
                        for (int i = 0; i < sbno.Length; i++)
                        {

                            string abno = sbno[i].ToString();

                            int rollno = Convert.ToInt32(abno);

                            var getstudent = db.VIEWGETSTUDENTATTs.Where(r => r.DIVISIONID == atteobj.DIVISIONID && r.ROLL_NO == rollno).ToList();
                            if (getstudent.Count == 0)
                            {
                                return new Results() { IsSuccess = false, Message = "No students found for this division " };

                            }
                            objDetail.ATTEDANCEMID = objmster.ATTEDANCEMID;
                            objDetail.ROLLNO = sbno[i].ToString();
                            objDetail.NAME = getstudent[0].STUDENTNAME;
                            objDetail.STUDENTID = getstudent[0].STUDENTID;

                            objDetail.STATUS = "A";

                            db.TBLATTENDENCEs.Add(objDetail);
                            db.SaveChanges();

                            string[] splitname = getstudent[0].STUDENTNAME.Split(' ');
                            TBLNOTIFICATIONDETAIL objnotidetails = new TBLNOTIFICATIONDETAIL();
                            objnotidetails.NOTIFICATIONID = objnotification.NOTIFICATIONID;
                            objnotidetails.STUDENTID = getstudent[0].STUDENTID;
                            objnotidetails.STATUS = 0;

                            db.TBLNOTIFICATIONDETAILs.Add(objnotidetails);
                            db.SaveChanges();

                            string str = "";
                            if (logindetail.UserName.StartsWith("NKV"))
                            {
                                str = "goo.gl/NsoiKY";
                            }
                            else if (logindetail.UserName.StartsWith("SXS"))
                            {
                                str = "goo.gl/zotf13";
                            }
                            else if (logindetail.UserName.StartsWith("ASM"))
                            {
                                str = "goo.gl/9vNiX8";
                            }

                            else if (logindetail.UserName.StartsWith("ASY"))
                            {
                                str = "goo.gl/SNtreT";
                            }
                            else if (logindetail.UserName.StartsWith("NMS"))
                            {
                                str = "goo.gl/j7XjCx";
                            }
                            string txtMessage = "Dear Parent, Your Pupil " + splitname[1] + ", is absent on " + Convert.ToDateTime(atteobj.ATTEDANCEDATE).ToString("dd/MM/yyyy") + ", Kindly note it. See attendance details " + str;


                            if (smsstatus == "1")
                            {

                                string responseString = SMSSendTESTDLR(getstudent[0].GMOBILE, txtMessage, logindetail.UserName);
                                if (responseString != "")
                                {
                                    var jObject = JObject.Parse(responseString);
                                    var response = jObject["response"].ToString();

                                    TBLMSGHISTORY smshist = new TBLMSGHISTORY();
                                    smshist.DATE = DateTime.Now;
                                    smshist.TIME = DateTime.Now.ToShortTimeString();
                                    smshist.MSG = txtMessage;
                                    smshist.TYPE = "ATT";
                                    smshist.CREATEDID = atteobj.Userid;
                                    smshist.DISPLAY = 1;
                                    smshist.STUDENTID = getstudent[0].STUDENTID;
                                    smshist.FROMEMPID = Convert.ToInt64(logindetail.EmpCode);
                                    smshist.STATUS = "Out";
                                    smshist.InStatus = "In";
                                    smshist.OutStatus = "Out";
                                    smshist.REQUESTID = response;
                                    smshist.EMPLOYEEID = 0;
                                    smshist.TOEMPID = "0";
                                    smshist.ATTACHMENTS = "";
                                    smshist.SUBJECT = "";
                                    smshist.OtherNos = "";
                                    smshist.ALUMNIID = 0;
                                    db.TBLMSGHISTORies.Add(smshist);
                                    db.SaveChanges();

                                }


                            }





                        



                            FCMPushNotification OBJPUSH = new FCMPushNotification();
                            //var getsubjectname = db.VIEWSUBJECTNAMEs.Where(r => r.SUBJECTID == obj.subject).ToList();

                            string studentid = Convert.ToString(getstudent[0].STUDENTID);
                            var userid = db.VIEWGETUSERIDFROMEMPCODEs.Where(r => r.EmpCode == studentid).FirstOrDefault();
                            var device = db.VW_DEVICE.FirstOrDefault(r => r.UserId == userid.UserId);
                            if (device != null)
                            {
                                if (!string.IsNullOrWhiteSpace(device.DeviceId))
                                    OBJPUSH.SendNotification("Attendance", string.Format("Dear Parent, Your pupil is absent on dated {0}, kindly note.", objmster.ATTEDANCEDATE.Value.ToString("dd-MM-yyyy")), device.DeviceId);
                            }
                        }


                        return new Results
                        {
                            IsSuccess = true,
                            Message = "Attendance Save successfully"
                        };

                    }

                    catch (Exception e)
                    {

                        return new Results
                        {
                            IsSuccess = false,
                            Message = e.Message
                        };

                    }


                }


                return new Results
                {
                    IsSuccess = false,
                    Message = "Attedance already taken for this Date"
                };

            }
            return new Results
            {
                IsSuccess = false,
                Message = "Class teacher is not assign to this division"
            };

        }
									   


        public string SMSSendTESTDLR(string MobileNo, string Message,string UserName)
        {
            try
            {

                string str = ""; string responseString = "";


                if (UserName.StartsWith("NKV"))
                {
                    str = "http://smsnow.hundiainfosys.com/rest/services/sendSMS/sendGroupSms?AUTH_KEY=7ddc928fc86d2e3adf01010536830d2&message=" + Message + "&senderId=SFNKVS&routeId=1&mobileNos=" + MobileNo + "&smsContentType=English";
                    

                }
                else if (UserName.StartsWith("SXS"))
                {
                    str = "http://smsnow.hundiainfosys.com/rest/services/sendSMS/sendGroupSms?AUTH_KEY=3555f03e24952528d1acba2e3f2e4749&message=" + Message + "&senderId=XAVIER&routeId=1&mobileNos=" + MobileNo + "&smsContentType=english";
                   
                }
                else if (UserName.StartsWith("ASM"))
                {

                    str = "http://smsnow.hundiainfosys.com/rest/services/sendSMS/sendGroupSms?AUTH_KEY=b963a0f8db5c6b3478df79dee5e5842e&message=" + Message + "&senderId=ALPHOS&routeId=1&mobileNos=" + MobileNo + "&smsContentType=english";
                }

                else if (UserName.StartsWith("ASY"))
                {
                  
                    str = "http://www.smsidea.co.in/sendsms.aspx?mobile=9923990000&pass=PKIGG&senderid=ALPHON&to=" + MobileNo + "&msg=" + Message + "";

                }
                else if (UserName.StartsWith("NMS"))
                {
                    //str = "http://smsnow.hundiainfosys.com/rest/services/sendSMS/sendGroupSms?AUTH_KEY=14c07610595093f4d66e18f1aac5ee88&message=" + Message + "&senderId=NMSKOP&routeId=1&mobileNos=" + MobileNo + "&smsContentType=english";

                }
                if (str != "")
                {
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(str);
                    HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                    System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                    responseString = respStreamReader.ReadToEnd();
                    respStreamReader.Close();
                    myResp.Close();
                }
                return responseString;
            }
            catch
            {
                return "";
            }
        }
        public object FileUpload(homeworkparameters obj)
       {

            int Userid = Convert.ToInt32(HttpContext.Current.Request["Userid"]);
            var Password = HttpContext.Current.Request["Password"];
            SchoolMainContext db = new ConcreateContext().GetContext(Userid, Password);
            if (db == null)
            {
                return new Results() { IsSuccess = false, Message = "Invalid User" };
            }
            var logindetail = db.TBLUSERLOGINs.
                              Where(r => r.UserId == Userid && r.Password == Password && r.STATUS == "ACTIVE")
                              .FirstOrDefault();
            var getUserType = db.VW_GET_USER_TYPE.Where(r => r.UserId == Userid).FirstOrDefault();
       
            try
            {
                ///Base Url
                string UploadBaseUrl = "";
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    if (logindetail.UserName.StartsWith("NKV"))
                    {
                        UploadBaseUrl = ConfigurationManager.AppSettings["NkvsBaseUrl"];
                    }
                    else if (logindetail.UserName.StartsWith("SXS"))
                    {
                        UploadBaseUrl = ConfigurationManager.AppSettings["StxavierBaseUrl"];
                    }
                    else if (logindetail.UserName.StartsWith("ASM"))
                    {
                        UploadBaseUrl = ConfigurationManager.AppSettings["AsmBaseUrl"];
                    }

                    else if (logindetail.UserName.StartsWith("ASY"))
                    {
                        UploadBaseUrl = ConfigurationManager.AppSettings["AsyBaseUrl"];
                    }
                    else if (logindetail.UserName.StartsWith("NMS"))
                    {
                        UploadBaseUrl = ConfigurationManager.AppSettings["NmsBaseUrl"];
                    }
                    string ftp = UploadBaseUrl;

                   //Upload File Using FTP
                    string ftpFolder = "UPLOADFILE/";
                    byte[] fileBytes = null;
                    string fileName = "";
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        fileName = postedFile.FileName;
                         using (StreamReader fileStream = new StreamReader(httpRequest.Files[file].InputStream))
                        {
                            fileBytes = Encoding.UTF8.GetBytes(fileStream.ReadToEnd());
                            fileStream.Close();
                        }

                        try
                        {
                            //Create FTP Request.
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp + ftpFolder + fileName);
                            request.Method = WebRequestMethods.Ftp.UploadFile;

                            //Enter FTP Server credentials.
                            request.Credentials = new NetworkCredential("akronsystems", "Password@123");
                            request.ContentLength = fileBytes.Length;
                            request.UsePassive = true;
                            request.UseBinary = true;
                            request.ServicePoint.ConnectionLimit = fileBytes.Length;
                            request.EnableSsl = false;
                            Stream requestStream = request.GetRequestStream();
                            requestStream.Write(fileBytes, 0, fileBytes.Length);
                            requestStream.Close();
                            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                            response.Close();


                            ////Save HomeWork To the Table
                            TBLHOMEWORK upload = new TBLHOMEWORK();
                            upload.STANDARDID = Convert.ToInt32(HttpContext.Current.Request["standardid"]);
                            upload.CREATEDID = int.Parse(getUserType.EmpCode);
                            upload.DIVISIONID = HttpContext.Current.Request["division"];
                            upload.SUBJECTID = Convert.ToInt32(HttpContext.Current.Request["subject"]);
                            upload.TERMID = Convert.ToInt32(HttpContext.Current.Request["term"]);
                            upload.HOMEWORK = HttpContext.Current.Request["homeworkdescription"];
                            upload.HOMEWORKDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                            upload.CREATEDON = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                            upload.TIME = DateTime.Now.ToShortTimeString();
                            upload.DISPLAY = 1;
                            upload.ACADEMICYEAR = "2018-2019";
                            upload.FILEPATH = fileName;
                            db.TBLHOMEWORKs.Add(upload);
                            db.SaveChanges();

                            ///Save HomeWork Notification




                            TBLNOTIFICATION objmaster = new TBLNOTIFICATION();
                            TBLNOTIFICATIONDETAIL objdetail = new TBLNOTIFICATIONDETAIL();

                            objmaster.TITLE = HttpContext.Current.Request["homeworkdescription"];
                            objmaster.NOTIFICATIONDATE = DateTime.Now;
                            objmaster.NOTIFICATIONTIME = DateTime.Now.ToShortTimeString();
                            objmaster.ACADEMICYEAR = "2018-2019";
                            objmaster.DIVISIONID = Convert.ToInt32(HttpContext.Current.Request["division"]);
                            objmaster.NOTIFICATIONTYPE = "Homework";
                            db.TBLNOTIFICATIONs.Add(objmaster);
                            db.SaveChanges();
                            objdetail.NOTIFICATIONID = objmaster.NOTIFICATIONID;

                            var DivisionId= Convert.ToInt32(HttpContext.Current.Request["division"]);
                            var getstudent = db.VIEWGETSTUDENTATTs.Where(r => r.DIVISIONID == DivisionId).ToList();
                            if (getstudent == null)
                            {

                                return new Results
                                {
                                    IsSuccess = false,
                                    Message = "Student Not Found"
                                };



                            }
                            // return getstudent;
                            for (int i = 0; i < getstudent.Count; i++)
                            {
                                TBLNOTIFICATIONDETAIL objnotidetails = new TBLNOTIFICATIONDETAIL();
                                objnotidetails.NOTIFICATIONID = objmaster.NOTIFICATIONID;
                                objnotidetails.STUDENTID = getstudent[i].STUDENTID;
                                objnotidetails.STATUS = 0;
                                db.TBLNOTIFICATIONDETAILs.Add(objnotidetails);
                                db.SaveChanges();
                                FCMPushNotification OBJPUSH = new FCMPushNotification();
                                //var getsubjectname = db.VIEWSUBJECTNAMEs.Where(r => r.SUBJECTID == obj.subject).ToList();

                                string studentid = Convert.ToString(getstudent[i].STUDENTID);
                                var userid = db.VIEWGETUSERIDFROMEMPCODEs.Where(r => r.EmpCode == studentid).FirstOrDefault();
                                var device = db.VW_DEVICE.FirstOrDefault(r => r.UserId == userid.UserId);
                                if (device != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(device.DeviceId))
                                        OBJPUSH.SendNotification("Homework", obj.homeworkdescription, device.DeviceId);
                                }
                                //if (smsstatus == "1")
                                //{
                                //    SMSSendTESTDLR(getstudent[i].GMOBILE, objHomework.HOMEWORK);
                                //}
                            }
                            return new Results
                            {

                                IsSuccess = true,
                                Message = "Homework assign successfully"
                            };
                        }
                        catch (WebException ex)
                        {
                            return new Results
                            {
                                IsSuccess = false,
                                Message = "Failed to upload File"
                            };
                        }
                    }



                   

                



                }
            }
            catch (Exception ex)
            {
                return new Results
                {
                    IsSuccess = false,
                    Message = ex.ToString()
                };
            }


            return null;
        }


    }
}