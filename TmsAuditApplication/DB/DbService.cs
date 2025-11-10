// File: DB/DbService.cs

using iTextSharp.text.pdf.qrcode;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Formats.Asn1;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.util;
using VrsAuditApplication.Models;
using System.Linq;
using System.Net.Sockets;


public class DbService
{
    private readonly string _connectionString;
    private static string CurrentDeviceIP = "";
    private static string _loginUserId = "";
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MyDb");
    }

    public void FetchData()
    {
        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            // Your SQL logic here
        }
    }

    public static void GetLocalIPAddress()
    {
        try
        {
            string? localIP = Dns.GetHostEntry(Dns.GetHostName())
                                .AddressList
                                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                                ?.ToString();

            CurrentDeviceIP = !string.IsNullOrEmpty(localIP)?localIP:"";
        }
        catch (Exception ex)
        {
            //return $"Error fetching IP: {ex.Message}";
        }
    }

    public SqlDataReader ExecuteStoredProcedureReader(string storedProcedureName, SqlParameter[] parameters)
    {
        if (parameters == null)
            throw new ArgumentException("Parameters cannot be null.");

        var nullParams = parameters.Where(p => p.Value == null).Select(p => p.ParameterName).ToList();

        if (nullParams.Any())
        {
            string nullParamList = string.Join(", ", nullParams);
            throw new ArgumentException($"Null parameter(s) detected: {nullParamList}. Ensure all parameters have valid values.");
        }

        SqlConnection con = new SqlConnection(_connectionString);
        SqlCommand cmd = new SqlCommand(storedProcedureName, con)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddRange(parameters);
        con.Open();

        return cmd.ExecuteReader(CommandBehavior.CloseConnection);
    }


    public SqlDataReader ExecuteReader(string query)
    {
        SqlConnection con = new SqlConnection(_connectionString);
        SqlCommand cmd = new SqlCommand(query, con);
        con.Open();
        return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
    }

    public SystemUser GetUserCredential(string userID)
    {
        SystemUser systemUser = null;

        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM [TMS_SM].[Admin].[SystemUsers] WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text; // ✅ Corrected

                    cmd.Parameters.AddWithValue("@UserID", userID);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            systemUser = new SystemUser
                            {
                                UserID = reader["UserID"].ToString(),
                                PlazaID = Convert.ToByte(reader["PlazaID"]),
                                PersonID = reader.IsDBNull(reader.GetOrdinal("PersonID")) ? null : reader.GetInt32(reader.GetOrdinal("PersonID")),
                                JobPositionID = reader["JobPositionID"] as string,
                                PasswordX = reader["PasswordX"]?.ToString(),
                                Password = reader["Password"]?.ToString(),
                                Active = Convert.ToBoolean(reader["Active"]),
                                RequestPasswordChange = reader["RequestPasswordChange"] as bool?,
                                PaymentMeansID = reader["PaymentMeansID"] as int?,
                                PasswordChangeDate = reader["PasswordChangeDate"] as DateTime?,
                                Login = Convert.ToBoolean(reader["Login"]),
                                SecurityKey = reader["SecurityKey"]?.ToString(),
                                PasswordExpiryDateTime = reader["PasswordExpiryDateTime"] as DateTime?,
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                ModifiedDate = Convert.ToDateTime(reader["ModifiedDate"]),
                                Status = Convert.ToBoolean(reader["Status"])
                            };
                        }
                        _loginUserId = systemUser?.UserID ?? ""; // Update static variable if user found
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // TODO: Add proper logging
             //Logger.LogError($"CheckUserCredential error: {ex.Message}", ex);
        }

        return systemUser;
    }
     
    public List<string> GetActiveUserIds()
    {
        List<string> userIds = new List<string>();

        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT UserID FROM [TMS_SM].[Admin].[SystemUsers] WHERE Active = 1 AND JobPositionID = 3";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userIds.Add(reader["UserID"].ToString().Trim());
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // TODO: Add proper logging
            // Logger.LogError($"GetActiveUserIds error: {ex.Message}", ex);
        }

        return userIds;
    }

    public Person GetPersonDetails(string userId)
    {
        Person personDetails = null;

        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM [TMS_SM].[Admin].[Persons] WHERE PersonID = (SELECT PersonID FROM [TMS_SM].[Admin].[SystemUsers] WHERE UserID = @UserID)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            personDetails = new Person
                            {
                                PersonID = reader.IsDBNull(reader.GetOrdinal("PersonID")) ? 0 : Convert.ToInt32(reader["PersonID"]),
                                PlazaID = reader.IsDBNull(reader.GetOrdinal("PlazaID")) ? 0 : Convert.ToInt32(reader["PlazaID"]),
                                JobPositionID = reader["JobPositionID"]?.ToString(),
                                IsPhysicalPerson = reader.IsDBNull(reader.GetOrdinal("IsPhysicalPerson")) ? 0 : Convert.ToInt32(reader["IsPhysicalPerson"]),
                                Salutation = reader["Salutation"]?.ToString(),
                                LastName = reader["LastName"]?.ToString() ?? string.Empty,
                                MiddleName = reader["MiddleName"]?.ToString(),
                                FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                                IDType = reader.IsDBNull(reader.GetOrdinal("IDType")) ? 0 : Convert.ToInt32(reader["IDType"]),
                                DocumentNumber = reader["DocumentNumber"]?.ToString() ?? string.Empty,
                                Sex = reader["Sex"]?.ToString() ?? string.Empty,
                                BirthPlace = reader["BirthPlace"]?.ToString(),
                                BirthDate = reader.IsDBNull(reader.GetOrdinal("BirthDate")) ? DateTime.MinValue : Convert.ToDateTime(reader["BirthDate"]),
                                Phone = reader["Phone"]?.ToString() ?? string.Empty,
                                Phone2 = reader["Phone2"]?.ToString(),
                                Fax = reader["Fax"]?.ToString(),
                                MobilePhone = reader["MobilePhone"]?.ToString(),
                                Email = reader["Email"]?.ToString() ?? string.Empty,
                                PersonAddressID = reader.IsDBNull(reader.GetOrdinal("PersonAddressID")) ? 0 : Convert.ToInt32(reader["PersonAddressID"]),
                                Active = !reader.IsDBNull(reader.GetOrdinal("Active")) && Convert.ToBoolean(reader["Active"]),
                                SmsAlert = !reader.IsDBNull(reader.GetOrdinal("SMS Alert")) && Convert.ToBoolean(reader["SMS Alert"]),
                                EmailAlert = !reader.IsDBNull(reader.GetOrdinal("Email Alert")) && Convert.ToBoolean(reader["Email Alert"]),
                                CreatedDate = reader.IsDBNull(reader.GetOrdinal("CreatedDate")) ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedDate"]),
                                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? DateTime.MinValue : Convert.ToDateTime(reader["ModifiedDate"]),
                                PasswordChangedBy = reader["PasswordChangedBy"]?.ToString() ?? string.Empty,
                                CreatedBy = reader["CreatedBy"]?.ToString(),
                                Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? 0 : Convert.ToInt32(reader["Status"])
                            };

                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // TODO: Add proper logging
            // Logger.LogError($"GetPersonDetails error: {ex.Message}", ex);
        }

        return personDetails;
    }



    public List<object> GetFastTagTransactionData(DateTime? fromDate, DateTime? toDate, string laneType, string tagClass, bool isManual, bool isViolation)
    {
        fromDate ??= DateTime.Today.AddDays(-1).AddSeconds(1);
        toDate ??= DateTime.Today.AddDays(1).AddSeconds(-1);

        var plazaID = 1;
        int parsedLaneType = 0;
        int.TryParse(laneType, out parsedLaneType);
        int parsedTagClass = 0;
        int.TryParse(tagClass, out parsedTagClass);
        int jobPosition = 0;
        int.TryParse("1", out jobPosition);
        var avcClassVal = 0;
        var bankClassVal = 0;

        List<object> transactions = new List<object>();

        try
        {
            //string procedureName = isManual
            //    ? "[TMS_SM].[dbo].[Validation_CCHGetManualFASTagTransaction]"
            //    : "[TMS_SM].[dbo].[Validation_CCHGetDiscrepencydata]";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@FromDate", fromDate.Value),
                new SqlParameter("@ToDate", toDate.Value),
                new SqlParameter("@PlazaID", plazaID),
                new SqlParameter("@LaneID", parsedLaneType)
            };

            string procedureName = "[TMS_SM].[dbo].[Validation_GetCCHdata]";
            if (isManual) {
                procedureName = "[TMS_SM].[dbo].[Validation_CCHGetActiveMapperTransaction]";
                parameters.Add(new SqlParameter("@TagClass", parsedTagClass));
                parameters.Add(new SqlParameter("@AVCClass", parsedTagClass));
                parameters.Add(new SqlParameter("@JobPositionID", jobPosition));
            }
            else if (isViolation)
            {
                procedureName = "[TMS_SM].[dbo].[Validation_CCHGetDiscrepencydata]";
            }
            else
            {
                parameters.Add(new SqlParameter("@TagClass", parsedTagClass));
                parameters.Add(new SqlParameter("@AVCClass", parsedTagClass));
                parameters.Add(new SqlParameter("@BankClass", parsedTagClass));
            }


            using (SqlDataReader reader = ExecuteStoredProcedureReader(procedureName, parameters.ToArray()))
            {
                while (reader.Read())
                {
                    if (isManual)
                    {
                        transactions.Add(new ManualFastTagTransaction
                        {
                            Tolltransactionnumber = reader["Tolltransactionnumber"]?.ToString(),
                            ActTransactionID = reader["ActTransactionID"]?.ToString(),
                            PlateNumber = reader["PlateNumber"]?.ToString(),
                            Laneid = reader["Laneid"]?.ToString(),
                            ShiftNumber = reader["ShiftNumber"]?.ToString(),
                            DateTime = reader["DateTime"]?.ToString(),
                            Tagid = reader["Tagid"]?.ToString(),
                            AVCClass = reader["AVCClass"]?.ToString(),
                            TagClass = reader["TagClass"]?.ToString(),
                            Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]).ToString("F2"): "0.00",
                            FareType = reader["FareType"]?.ToString(),
                            AVCClassID = reader["AVCClassID"]?.ToString(),
                            IsValidated = reader["IsValidated"] != DBNull.Value && Convert.ToBoolean(reader["IsValidated"])
                        });
                    }
                    else if(isViolation)
                    {
                        transactions.Add(new FastTagTransaction
                        {
                            Tolltransactionnumber = reader["Tolltransactionnumber"]?.ToString(),
                            ActTransactionID = reader["ActTransactionID"]?.ToString(),
                            PlateNumber = reader["PlateNumber"]?.ToString(),
                            Laneid = reader["Laneid"]?.ToString(),
                            ShiftNumber = reader["ShiftNumber"]?.ToString(),
                            DateTime = reader["DateTime"]?.ToString(),
                            Tagid = reader["Tagid"]?.ToString(),
                            AVCClass = reader["AVCClass"]?.ToString(),
                            TagClass = reader["TagClass"]?.ToString(),
                            Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]).ToString("F2") : "0.00",
                            FareType = reader["FareType"]?.ToString(),
                            AVCClassID = reader["AVCClassID"]?.ToString()
                        });
                    }else
                    {
                        transactions.Add(new FastTagTransaction
                        {
                            Tolltransactionnumber = reader["Tolltransactionnumber"]?.ToString(),
                            ActTransactionID = reader["ActTransactionID"]?.ToString(),
                            PlateNumber = reader["PlateNumber"]?.ToString(),
                            Laneid = reader["Laneid"]?.ToString(),
                            ShiftNumber = reader["ShiftNumber"]?.ToString(),
                            DateTime = reader["DateTime"]?.ToString(),
                            Tagid = reader["Tagid"]?.ToString(),
                            AVCClass = reader["AVCClass"]?.ToString(),
                            TagClass = reader["TagClass"]?.ToString(),
                            Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]).ToString("F2") : "0.00",
                            FareType = reader["FareType"]?.ToString(),
                            AVCClassID = reader["AVCClassID"]?.ToString()
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //throw new Exception("Error in GetFastTagTransactionData: " + ex.Message, ex);
        }

        return transactions;
    }


    //public void SI_Accepted(bool chkIsManual, bool cbIsVoilation, string TransactionNumber)
    //{
    //    if (cbIsVoilation)
    //    {
    //        if (cmbAuditClass.SelectedIndex == 0)
    //        {
    //            MessageBox.Show("Please select Audited Vehicle Class", "Update Violation Class", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //            return;
    //        }

    //        Parameters = new Hashtable();
    //        Parameters.Add("@Transactionid", TransactionNumber);
    //        Parameters.Add("@ViolationClass", cmbAuditClass.SelectedValue);
    //        //int result =
    //        dbConnect.RunProcedureInput("[dbo].[Validation_Insert_CCHViolationTxn]", Parameters);

    //        //if (result != 1)
    //        //{
    //        //    MessageBox.Show("Error Occured While Updating Details. Try Again Later", "Update AVC", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //        //}
    //        //else
    //        {
    //            hiderow();
    //        }
    //    }
    //    if (chkIsManual)
    //    {
    //        try
    //        {
    //            Parameters = new Hashtable();
    //            Parameters.Add("@Tolltransactionnumber", TransactionNumber);
    //            Parameters.Add("@isvalidate", 3);
    //            dbConnect.RunProcedureInput("[dbo].[Validation_UpdateManulaFASTag]", Parameters);
    //            hiderow();
    //        }
    //        catch (Exception ex)
    //        {
    //            LogException.ExceptionSQLLog("Audit Acceptance() \n" + ex.Source, ex.StackTrace, ex.Message);
    //        }
    //    }
    //}

    //public void SI_Rejected(bool cbIsVoilation, bool chkIsManual, string TransactionNumber, int AVCClassID)
    //{
    //    string jobid="0";
    //    if (jobid == "3")
    //    {
    //        if (chkIsManual.Checked)
    //        {
    //            try
    //            {
    //                Parameters = new Hashtable();
    //                Parameters.Add("@Tolltransactionnumber", TransactionNumber);
    //                Parameters.Add("@isvalidate", 4);
    //                dbConnect.RunProcedureInput("[dbo].[Validation_UpdateManulaFASTag]", Parameters);
    //                hiderow();
    //            }
    //            catch (Exception ex)
    //            {
    //                LogException.ExceptionSQLLog("Audit Rejection() \n" + ex.Source, ex.StackTrace, ex.Message);
    //            }
    //        }
    //        if (cbIsVoilation.Checked)
    //        {
    //            try
    //            {

    //                Parameters = new Hashtable();
    //                Parameters.Add("@TransactionNumber", TransactionNumber);
    //                Parameters.Add("@BankClass", BankClassID);
    //                Parameters.Add("@AVCClass", AVCClassID);
    //                int result = dbConnect.RunProcedureInputWithOutput("[dbo].[Validation_UpdateViolationtxnAVC_Class]", Parameters);

    //                if (result != 1)
    //                {
    //                    MessageBox.Show("Error Occured While Updating Details. Try Again Later", "Update AVC", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //                }
    //                else
    //                {
    //                    hiderow();
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                LogException.ExceptionSQLLog("AVC Reject \n" + ex.Source, ex.StackTrace, ex.Message);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        try
    //        {
    //            Parameters = new Hashtable();
    //            Parameters.Add("@Tolltransactionnumber", TransactionNumber);
    //            Parameters.Add("@isvalidate", 4);
    //            dbConnect.RunProcedureInput("[dbo].[Validation_UpdateManulaFASTag]", Parameters);
    //            hiderow();
    //        }
    //        catch (Exception ex)
    //        {
    //            LogException.ExceptionSQLLog("SI Rejection() \n" + ex.Source, ex.StackTrace, ex.Message);
    //        }
    //    }
    //}


    public List<NonFastTagTransaction> GetNonFastTagTransactionData(string category,string subCategory,int tcClass,int avcClass,DateTime? fromDate, DateTime? toDate, string userId,int laneId,string shiftType)
    {
        List<NonFastTagTransaction> transactions = new List<NonFastTagTransaction>();

        try
        {
            fromDate ??= DateTime.Today.AddDays(-1).AddSeconds(1);
            toDate ??= DateTime.Today.AddDays(1).AddSeconds(-1);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[Validation_SP].[Validation_TransactionCategory]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PlazaID", 1);
                    cmd.Parameters.AddWithValue("@ShiftNumber", 0);
                    cmd.Parameters.AddWithValue("@Category", category ?? "All");
                    cmd.Parameters.AddWithValue("@SubCategory", subCategory ?? "All");
                    cmd.Parameters.AddWithValue("@TCClass", tcClass);
                    cmd.Parameters.AddWithValue("@AVCClass", avcClass);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@UserID", userId ?? "All");
                    cmd.Parameters.AddWithValue("@LaneID", laneId);
                    cmd.Parameters.AddWithValue("@ShiftType", shiftType ?? "All");
                    cmd.Parameters.AddWithValue("@PMT", 0);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new NonFastTagTransaction
                            {
                                TollTransactionNumber = reader["TollTransactionNumber"] != DBNull.Value ? Convert.ToInt64(reader["TollTransactionNumber"]) : 0,
                                ShiftNumber = reader["ShiftNumber"] != DBNull.Value ? Convert.ToInt32(reader["ShiftNumber"]) : 0,
                                LaneID = reader["LaneID"] != DBNull.Value ? Convert.ToInt32(reader["LaneID"]) : 0,
                                DateTime = reader["DateTime"] != DBNull.Value ? Convert.ToDateTime(reader["DateTime"]) : DateTime.MinValue,
                                ReceiptNumber = reader["ReceiptNumber"] != DBNull.Value ? Convert.ToInt64(reader["ReceiptNumber"]) : 0,
                                Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                                TCClass = reader["TCClass"] != DBNull.Value ? Convert.ToInt32(reader["TCClass"]) : 0,
                                AVCClass = reader["AVCClass"] != DBNull.Value ? Convert.ToInt32(reader["AVCClass"]) : 0,
                                PaymentMeansType = reader["PaymentMeansType"] != DBNull.Value ? Convert.ToInt32(reader["PaymentMeansType"]) : 0,
                                //PaymentMode = reader["PaymentMode"]?.ToString(),
                                ExemptionID = reader["ExemptionID"] != DBNull.Value ? Convert.ToInt32(reader["ExemptionID"]) : 0,
                                PlateNumber = reader["Platenumber"]?.ToString(),
                                Simulation = reader["Simulation"] != DBNull.Value && Convert.ToBoolean(reader["Simulation"]),
                                FareID = reader["FareID"] != DBNull.Value ? Convert.ToInt32(reader["FareID"]) : 0,
                                SWBWeight = reader["SWB Weight"] != DBNull.Value ? Convert.ToInt32(reader["SWB Weight"]) : 0,
                                SWBAmount = reader["SWB Amount"] != DBNull.Value ? Convert.ToDecimal(reader["SWB Amount"]) : 0,
                                IsUnderWeight = reader["IsUnderWieght"]?.ToString(),
                                UserID = reader["UserID"] != DBNull.Value ? Convert.ToInt32(reader["UserID"]) : 0,
                                ValidatorClass = reader["validatorclass"]?.ToString(),
                                BagNumber = reader["BagNumber"]?.ToString()
                            });
                        }
                    }
                }
            }

        }
        catch(Exception ex)
        {
            //throw new Exception("Error in GetNonFastTagTransactionData: " + ex.Message, ex);
        }

        return transactions;

    }


    public void AuditorAcceptUpdate(string transactionNumber, bool isMannual, int avcClassId)
    {
        if (!isMannual)
        {
            InsertTxn(0, transactionNumber, avcClassId);
        }
        else
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("[TMS_SM].[dbo].[Validation_UpdateManulaFASTag]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Tolltransactionnumber", transactionNumber);
                        cmd.Parameters.AddWithValue("@isvalidate", 3);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
               // throw new Exception("Audit Acceptance() \n" + ex.Source, ex.StackTrace, ex.Message);
            }
        }
    }

    private void InsertTxn(int Mode, string transactionNumber, int avcClassId)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[Validation_SP].[Validation_CCHupdatevalidatorclass]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    //cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                    //cmd.Parameters.AddWithValue("@Validator", "Operation");
                    //cmd.Parameters.AddWithValue("@ValidatorClass", bankClassId);

                    cmd.Parameters.AddWithValue("@transactionid", transactionNumber);
                    cmd.Parameters.AddWithValue("@updatedavclass", avcClassId);
                    cmd.Parameters.AddWithValue("@mode", Mode);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

        }
        catch (Exception ex)
        {
            //
        }

    }

    public void UpdateAcceptTC_NonFastTag(int plazaID,
                                    string transactionNumber,
                                    string shiftNumber,
                                    string validator,
                                    string exemptionID,
                                    string tccClass,
                                    string avcClass,
                                    string validatorClass,
                                    string fareID,
                                    string paymentMeansType,
                                    string shiftvalidationMode,
                                    string remark)
    {


        InsertTxnNonFastTag("TollCollector", plazaID, transactionNumber, shiftNumber, validator, exemptionID, tccClass, avcClass, validatorClass, fareID, paymentMeansType, shiftvalidationMode, remark);
        
    }

    public void InsertTxnNonFastTag(
                                    string mode,
                                    int plazaID,
                                    string transactionNumber,
                                    string shiftNumber,
                                    string validator,
                                    string exemptionID,
                                    string tccClass,
                                    string avcClass,
                                    string validatorClass,
                                    string fareID,
                                    string paymentMeansType,
                                    string shiftvalidationMode,
                                    string remark)
    
    {
        try
        {

            if (string.IsNullOrEmpty(remark))
                remark =  "N/A";
            return;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[Validation_SP].[Validation_ManualValidation]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Mode", mode);
                    cmd.Parameters.AddWithValue("@PlazaID", plazaID);
                    cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                    cmd.Parameters.AddWithValue("@ShiftNumber", shiftNumber);
                    cmd.Parameters.AddWithValue("@Validator", _loginUserId);
                    cmd.Parameters.AddWithValue("@ExemptionID", exemptionID);
                    cmd.Parameters.AddWithValue("@TCClass", tccClass);
                    cmd.Parameters.AddWithValue("@AVCClass", avcClass);
                    cmd.Parameters.AddWithValue("@ValidatorClass", "");
                    cmd.Parameters.AddWithValue("@FareID", fareID);
                    cmd.Parameters.AddWithValue("@PaymentMeansType", paymentMeansType);
                    cmd.Parameters.AddWithValue("@validationMode", shiftvalidationMode);
                    cmd.Parameters.AddWithValue("@Remark", remark);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            //LogException.ExceptionSQLLog("InsertTxnNonFastTag\r\n() \n" + ex.Source, ex.StackTrace, ex.Message);
        }

    }

    public void UpdateViolationTxnAudit_NonFastTag(
                                    int plazaID,
                                    string transactionNumber,
                                    string shiftNumber,
                                    string plateNumber,
                                    string vehicleClass,
                                    string shiftvalidationMode,
                                    string subClass,
                                    string violationType,
                                    string remark)

    {
        try
        {

            if (string.IsNullOrEmpty(remark))
                remark =  "N/A";
            return;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[Validation_SP].[Validation_UpdateViolationTxn]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PlazaID", plazaID);
                    cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                    cmd.Parameters.AddWithValue("@ShiftNumber", shiftNumber);
                    cmd.Parameters.AddWithValue("@PlateNumber", plateNumber);
                    cmd.Parameters.AddWithValue("@MakeID", 0);
                    cmd.Parameters.AddWithValue("@ModelID", string.Empty);
                    cmd.Parameters.AddWithValue("@ColorID", 0);
                    cmd.Parameters.AddWithValue("@Validator", _loginUserId);
                    cmd.Parameters.AddWithValue("@ValidatorClass", "");
                    cmd.Parameters.AddWithValue("@ClassID", vehicleClass);
                    cmd.Parameters.AddWithValue("@SubClassID", subClass);
                    cmd.Parameters.AddWithValue("@ViolationType", violationType);
                    cmd.Parameters.AddWithValue("@validationMode", shiftvalidationMode);
                    cmd.Parameters.AddWithValue("@remark", remark);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            //LogException.ExceptionSQLLog("InsertTxnNonFastTag\r\n() \n" + ex.Source, ex.StackTrace, ex.Message);
        }

    }

    public void UpdateAudit_NonFastTag(
                                    int plazaID,
                                    string transactionNumber,
                                    string shiftNumber,
                                    string exemptionID,
                                    string tccClass,
                                    string avcClass,
                                    string validatorClass,
                                    string fareID,
                                    string paymentMeansType,
                                    string shiftvalidationMode,
                                    string subClass,
                                    string violationType,
                                    string remark)

    {
        try
        {

            if (string.IsNullOrEmpty(remark))
                remark =  "N/A";
            return;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[Validation_SP].[Validation_ManualValidation]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Mode", "Audit");
                    cmd.Parameters.AddWithValue("@PlazaID", plazaID);
                    cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                    cmd.Parameters.AddWithValue("@ShiftNumber", shiftNumber);
                    cmd.Parameters.AddWithValue("@Validator", _loginUserId);
                    cmd.Parameters.AddWithValue("@ExemptionID", exemptionID);
                    cmd.Parameters.AddWithValue("@TCClass", tccClass);
                    cmd.Parameters.AddWithValue("@AVCClass", avcClass);
                    cmd.Parameters.AddWithValue("@ValidatorClass", validatorClass);
                    cmd.Parameters.AddWithValue("@FareID", fareID);
                    cmd.Parameters.AddWithValue("@PaymentMeansType", paymentMeansType);
                    cmd.Parameters.AddWithValue("@validationMode", shiftvalidationMode);

                    if (paymentMeansType == "5")
                    {
                        cmd.Parameters.AddWithValue("@ViolationType", violationType);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ViolationType", 0);
                    }

                    cmd.Parameters.AddWithValue("@SubClass", subClass);
                    if (remark.Length == 0)
                        cmd.Parameters.AddWithValue("@Remark", "N/A");
                    else
                    cmd.Parameters.AddWithValue("@Remark", remark);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            //LogException.ExceptionSQLLog("InsertTxnNonFastTag\r\n() \n" + ex.Source, ex.StackTrace, ex.Message);
        }

    }

    public void AvcAudit_NonFastTag(
                                    int plazaID,
                                    string transactionNumber,
                                    string shiftNumber,
                                    string exemptionID,
                                    string tccClass,
                                    string avcClass,
                                    string validatorClass,
                                    string fareID,
                                    string paymentMeansType,
                                    string shiftvalidationMode,
                                    string subClass,
                                    string violationType,
                                    string remark)

    {
        try
        {

            if (string.IsNullOrEmpty(remark))
                remark =  "N/A";
            return;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[Validation_SP].[Validation_ManualValidation]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Mode", "AVC");
                    cmd.Parameters.AddWithValue("@PlazaID", plazaID);
                    cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                    cmd.Parameters.AddWithValue("@ShiftNumber", shiftNumber);
                    cmd.Parameters.AddWithValue("@Validator", _loginUserId);
                    cmd.Parameters.AddWithValue("@ExemptionID", exemptionID);
                    cmd.Parameters.AddWithValue("@TCClass", tccClass);
                    cmd.Parameters.AddWithValue("@AVCClass", avcClass);
                    cmd.Parameters.AddWithValue("@ValidatorClass", validatorClass);
                    cmd.Parameters.AddWithValue("@FareID", fareID);
                    cmd.Parameters.AddWithValue("@PaymentMeansType", paymentMeansType);
                    cmd.Parameters.AddWithValue("@validationMode", shiftvalidationMode);

                    if (paymentMeansType == "5")
                    {
                        cmd.Parameters.AddWithValue("@ViolationType", violationType);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ViolationType", 0);
                    }

                    cmd.Parameters.AddWithValue("@SubClass", subClass);
                    if (remark.Length == 0)
                        cmd.Parameters.AddWithValue("@Remark", "N/A");
                    else
                        cmd.Parameters.AddWithValue("@Remark", remark);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            //LogException.ExceptionSQLLog("InsertTxnNonFastTag\r\n() \n" + ex.Source, ex.StackTrace, ex.Message);
        }

    }


    public void AuditorRejectUpdate(string transactionNumber, bool isMannual, int avcClassId)
    {
        if (!isMannual)
        {
            InsertTxn(2, transactionNumber, avcClassId);
        }
        else
        {
            // 🔹 Directly execute the manual reject SP
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("[TMS_SM].[dbo].[Validation_UpdateManulaFASTag]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Tolltransactionnumber", transactionNumber);
                        cmd.Parameters.AddWithValue("@isvalidate", 4);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                //
            }
        }
    }



    public List<FloatDetail> GetFloatDetails(DateTime operationDay, string userName)
    {
        var floatDetails = new List<FloatDetail>();

        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[CashUp_sp].[Cashup_GetOpeariondayUsers]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OperationDay", operationDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@UserID", userName);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            floatDetails.Add(new FloatDetail
                            {
                                UserName     = reader["User Name"].ToString(),
                                SealNumber   = reader["Seal Number"].ToString(),
                                FloatDateTime = Convert.ToDateTime(reader["Float Date Time"]),
                                UserId       = reader["Userid"].ToString(),
                                BagNumber    = reader["Bagnumber"].ToString()
                            });
                        }

                    }
                }
            }
        }
        catch (Exception ex)
        {
            // TODO: Proper logging
        }

        return floatDetails;
    }


    public bool InsertFloatAmount(string userId, int bagNumber, double floatAmount, DateTime operationDay, string shift)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("[TMS_SM].[CashUp_sp].[Cashup_InsertFloatAmount]", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters
                cmd.Parameters.Add("@UserID", SqlDbType.VarChar).Value = userId;
                cmd.Parameters.Add("@BagNumber", SqlDbType.Int).Value = bagNumber;
                cmd.Parameters.Add("@FloatAmount", SqlDbType.Float).Value = floatAmount;
                cmd.Parameters.Add("@AssignerName", SqlDbType.VarChar).Value = _loginUserId;
                cmd.Parameters.Add("@OperationDay", SqlDbType.DateTime).Value = operationDay.ToString("yyyy-MM-dd HH:mm:ss");
                cmd.Parameters.Add("@Shift", SqlDbType.Char, 1).Value = shift;

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0; // ✅ True if insert successful
            }
        }
        catch (Exception ex)
        {
            // TODO: add proper logging
            throw new Exception("Error inserting float amount", ex);
        }
    }


    public bool InsertFloatAmountTopup(string userId, int bagNumber, double topupAmount, DateTime operationDay, string shift, int active)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("[TMS_SM].[CashUp_sp].[Cashup_InsertFloatAmountTopup]", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters
                cmd.Parameters.Add("@UserID", SqlDbType.VarChar).Value = userId;
                cmd.Parameters.Add("@BagNumber", SqlDbType.Int).Value = bagNumber;
                cmd.Parameters.Add("@TopupAmount", SqlDbType.Float).Value = topupAmount;
                cmd.Parameters.Add("@AssignerName", SqlDbType.VarChar).Value = _loginUserId;
                cmd.Parameters.Add("@OperationDay", SqlDbType.DateTime).Value = operationDay.ToString("yyyy-MM-dd HH:mm:ss");
                cmd.Parameters.Add("@Shift", SqlDbType.Char, 1).Value = shift;
                cmd.Parameters.Add("@Active", SqlDbType.Int).Value = active;

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0; // ✅ True if insert successful
            }
        }
        catch (Exception ex)
        {
            // TODO: add proper logging
            throw new Exception("Error inserting float top-up amount", ex);
        }
    }

    public bool UpdateCancelFloat(string userId, int bagNumber)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("[TMS_SM].[CashUp_sp].[Cashup_UpdateCancelFloat]", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters
                cmd.Parameters.Add("@Bagnumber", SqlDbType.Int).Value = bagNumber;
                cmd.Parameters.Add("@UserID", SqlDbType.VarChar).Value = userId;
                cmd.Parameters.Add("@CanceledUserid", SqlDbType.VarChar).Value = _loginUserId;

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0; // ✅ True if updated successful
            }
        }
        catch (Exception ex)
        {
            // TODO: add proper logging
            throw new Exception("Error updating cancel float.", ex);
        }
    }

    public List<ShiftDetails> GetShiftDetails(string user, int bagNumber)
    {

        List<ShiftDetails> shifts = new List<ShiftDetails>();

        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[CashUp_sp].[Cashup_GetTotalShiftDetail]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserID", user);
                    cmd.Parameters.AddWithValue("@BagNo", bagNumber);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var shift = new ShiftDetails
                            {
                                ShiftNumber = reader["Shift Number"]?.ToString(),
                                ShiftOrigin = reader["ShiftOrigin"]?.ToString(),
                                UserName = reader["User Name"]?.ToString(),
                                StartOfShiftTime = reader["Start Of Shift Time"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["Start Of Shift Time"])
                                    : DateTime.MinValue,
                                EndOfShiftTime = reader["End Of Shift Time"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["End Of Shift Time"])
                                    : DateTime.MinValue,
                                BagNumber = Convert.ToInt32(reader["Bag Number"]),
                                OperationDay = reader["Operation Day"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["Operation Day"])
                                    : DateTime.MinValue
                            };

                            shifts.Add(shift);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // log error here properly
            throw new Exception("Error in GetShiftDetails.", ex);
        }

        return shifts;
    }

    public List<PendingCashupDetail> GetPendingCashupDetails()
    {
        var pendingCashups = new List<PendingCashupDetail>();

        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[CashUp_sp].[Cashup_GetPendingBagNumber]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure; // ✅ Important

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detail = new PendingCashupDetail
                            {
                                UserName      = reader["User Name"]?.ToString().Trim(),
                                BagNo         = reader["Bag No"]?.ToString(),
                                Shift         = reader["Shift"]?.ToString(),
                                FloatAmount   = reader["Float  Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Float  Amount"]) : 0,
                                OperationDay  = reader["Operation Day"] != DBNull.Value ? Convert.ToDateTime(reader["Operation Day"]) : DateTime.MinValue,
                                FloatDateTime = reader["Float DateTime"] != DBNull.Value ? Convert.ToDateTime(reader["Float DateTime"]) : DateTime.MinValue,
                                UserID        = reader["UserID"]?.ToString().Trim()
                            };

                            pendingCashups.Add(detail);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // TODO: replace with ILogger or proper logging
            Console.WriteLine("Error in GetPendingCashupDetails: " + ex.Message);
        }

        return pendingCashups;
    }




    public (bool success, string message) ValidateCashup(DeclareCashupModel model)
    {
        //return (true, "Invalid seal number.");

        try
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("[TMS_SM].[CashUp_sp].[Cashup_DataCompleteness]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Bagnum", Convert.ToInt32(model.BagNumber));

                        using (SqlDataAdapter da2 = new SqlDataAdapter(cmd))
                        {
                            DataSet ds2 = new DataSet();
                            da2.Fill(ds2);

                            if (ds2.Tables[0].Rows[0][0].ToString() != "Complete")
                                return (false, "Data is not complete. You can't cashup.");
                        }
                    }

                }
                catch (Exception ex)
                {
                    return (false, "Data Complateness Check Issue! \r\n An error occurred: " + ex.Message);
                }

                // 1️⃣ Validate Bag Number
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[Cashup_sp].[Cashup_SealNoBleedNoValidationCheck]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", model.UserName);
                    cmd.Parameters.AddWithValue("@Bagnumber", Convert.ToInt64(model.BagNumber));

                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                            return (false, "Invalid seal number.");

                        int bagcount = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
                        int Shiftopening = Convert.ToInt32(ds.Tables[0].Rows[0][1].ToString());
                        int BleedCount = Convert.ToInt32(ds.Tables[0].Rows[0][2].ToString());
                        int ShiftCount = Convert.ToInt32(ds.Tables[0].Rows[0][3].ToString());
                        int Validation = Convert.ToInt32(ds.Tables[0].Rows[0][4].ToString());
                        int Cashuppending = Convert.ToInt32(ds.Tables[0].Rows[0][5].ToString());
                        int Plazasupervisor = Convert.ToInt32(ds.Tables[0].Rows[0][6].ToString());
                        int PlazasupervisorShiftCheck = Convert.ToInt32(ds.Tables[0].Rows[0][7].ToString());
                        int ShiftGeneratedCheck = Convert.ToInt32(ds.Tables[0].Rows[0][8].ToString());
                        int floatcheck = Convert.ToInt32(ds.Tables[0].Rows[0]["FloatCheck"].ToString());
                        int verified = Convert.ToInt32(ds.Tables[0].Rows[0]["Verified"].ToString());
                        
                        if (bagcount != 1)
                        {

                            using (SqlCommand cmd1 = new SqlCommand("[TMS_SM].[Cashup_sp].[Cashup_GetUserBagNumber]", con))
                            {
                                cmd1.CommandType = CommandType.StoredProcedure;
                                cmd1.Parameters.AddWithValue("@UserID", model.UserName.Trim());

                                Hashtable Parameters = new Hashtable();
                                Parameters.Add("@UserID", model.UserName.Trim());
                                using (SqlDataAdapter da1 = new SqlDataAdapter(cmd))
                                {
                                    DataSet ds1 = new DataSet();
                                    da1.Fill(ds1);
                                    ArrayList BagNoAr = new ArrayList();
                                    string Totalassignedbag = "";
                                    StringBuilder st = new StringBuilder();
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                        {
                                            st.Append(ds.Tables[0].Rows[i][0].ToString());
                                            if (i != (ds.Tables[0].Rows.Count - 1))
                                                st.Append(",");
                                            Totalassignedbag = st.ToString();
                                        }
                                    }
                                    return (false, "Bag Number Does Not Match with Assigned Seal Number  " + Totalassignedbag);
                                }
                            }

                        }
                        if (floatcheck == 0)
                        {
                            return (false, "Float has been canceled for this number! Cashup cannot be done");
                        }
                        if (verified != 0)
                        {
                            return (false, "Bag Number are pending to verified! Please Verified your bagnumber !");
                        }
                        if (ShiftGeneratedCheck == 0)
                        {
                            return (false, "There is no shift generated! \r\n Please generated the shifts or cancel the float!");
                        }
                        if (Cashuppending != 0)
                        {
                            return (false, "No Pending Cashup for this Seal Number");
                        }
                        if (BleedCount != 0)
                        {
                            return (false, "Bleed is pending for this Seal Number");
                        }
                        if (Plazasupervisor >= 1)
                        {
                            if (PlazasupervisorShiftCheck >= 1)
                            {
                                return (false, "Please closed your shifts from the Dashboard! Before that you cannot do the cashup.");
                            }
                        }
                        //if (Shiftopening >= 1)
                        //{
                        //    DialogResult rs = MessageBox.Show("Your Shift is Running Do you want to close it ?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        //    if (rs == DialogResult.Yes)
                        //    {
                        //        try
                        //        {
                        //            Hashtable Parameters = new Hashtable();
                        //            ds = new DataSet();
                        //            Parameters.Add("@UserID", model.UserName.Trim());
                        //            Parameters.Add("@Bagnum", Convert.ToInt32(model.BagNumber.Trim()));
                        //            dbConnect.RunProcedureInput("[CashUp_sp].[Cashup_Close AllRunningShift]", Parameters);

                        //            MessageBox.Show("Your Shift has closed successfully for this user", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //            //txtDrawerNo.Text = "";
                        //            InsertEvent("User declaration seal number accepted by system and ready for cashup declaration for this seal number :-" + txtDrawerNo.Text.Trim() + " and for this user name :" + GlobalValues.Username);
                        //            FrmCashupDeclare1 DeclareScren = new FrmCashupDeclare1(Convert.ToInt32(txtDrawerNo.Text.Trim()), operationdaypick.Value.ToString("yyyy-MM-dd"), _operationDay, cmbShift.SelectedItem.ToString(), txtnameuser.Text.Trim());
                        //            //DeclareScren.MdiParent = this.MdiParent;
                        //            DeclareScren.WindowState = FormWindowState.Maximized;
                        //            DeclareScren.StartPosition = FormStartPosition.CenterScreen;
                        //            DeclareScren.ShowDialog();
                        //            this.Close();
                        //            return;
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            LogException.ExceptionLog(ex.Source, ex.Message, ex.StackTrace);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        return;
                        //    }
                        //}
                        if (ShiftCount != 0)
                        {
                            return (false, "Please check you validation! Some shifts for validation for this seal number are pending !");
                        }
                        if (Validation != 0)
                        {
                            return (false, "Please check you validation! Transaction for this seal number validation are pending !");
                        }

                        //try
                        //{
                        //    Parameters = new Hashtable();
                        //    ds = new DataSet();
                        //    Parameters.Add("@UserID", txtnameuser.Text.Trim());
                        //    Parameters.Add("@Bagnum", Convert.ToInt32(txtDrawerNo.Text.Trim()));
                        //    ds = dbConnect.RunProcedureDatasetInput("[CashUp_sp].[Cashup_CheckFloatAcceptance]", Parameters);

                        //    if (ds.Tables[0].Rows[0][0].ToString() == "0")
                        //    {
                        //        if (MessageBox.Show("Do you want to return float amount?", "Return Float", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                        //        {
                        //            dbConnect.RunProcedureInput("[CashUp_sp].[Cashup_AcceptFloat]", Parameters);

                        //            FrmCashupDeclare1 DeclareScren = new FrmCashupDeclare1(Convert.ToInt32(txtDrawerNo.Text.Trim()), operationdaypick.Value.ToString("yyyy-MM-dd"), _operationDay, cmbShift.SelectedItem.ToString(), txtnameuser.Text.Trim());
                        //            //DeclareScren.MdiParent = this.MdiParent;
                        //            DeclareScren.WindowState = FormWindowState.Maximized;
                        //            DeclareScren.StartPosition = FormStartPosition.CenterScreen;
                        //            DeclareScren.ShowDialog();
                        //            this.Close();
                        //        }
                        //    }
                        //    else if (ds.Tables[0].Rows[0][0].ToString() == "1")
                        //    {
                        //        FrmCashupDeclare1 DeclareScren = new FrmCashupDeclare1(Convert.ToInt32(txtDrawerNo.Text.Trim()), operationdaypick.Value.ToString("yyyy-MM-dd"), _operationDay, cmbShift.SelectedItem.ToString(), txtnameuser.Text.Trim());
                        //        //DeclareScren.MdiParent = this.MdiParent;
                        //        DeclareScren.WindowState = FormWindowState.Maximized;
                        //        DeclareScren.StartPosition = FormStartPosition.CenterScreen;
                        //        DeclareScren.ShowDialog();
                        //        this.Close();
                        //    }

                        //    //InsertEvent("User declaration seal number accepted by system and ready for cashup declaration for this seal number :-" + txtDrawerNo.Text.Trim() + " and for this user name :" + GlobalValues.Username);

                        //    return;
                        //}
                        //catch (Exception ex)
                        //{
                        //    LogException.ExceptionLog(ex.Source, ex.Message, ex.StackTrace);
                        //    return;
                        //}

                        //InsertEvent("User declaration seal number accepted by system and ready for cashup declaration for this seal number :-" + txtDrawerNo.Text.Trim() + " and for this user name :" + GlobalValues.Username);
                        //FrmCashupDeclare1 DeclareScren1 = new FrmCashupDeclare1(Convert.ToInt32(txtDrawerNo.Text.Trim()), _operationDay, _operationDay, cmbShift.SelectedItem.ToString(), txtnameuser.Text.Trim());
                        ////DeclareScren1.MdiParent = this.MdiParent;
                        //DeclareScren1.WindowState = FormWindowState.Maximized;
                        //DeclareScren1.StartPosition = FormStartPosition.CenterScreen;
                        //DeclareScren1.ShowDialog();
                        //this.Close();

                    }
                }

                
                // 2️⃣ Check Float Acceptance
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[CashUp_sp].[Cashup_CheckFloatAcceptance]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", model.UserName);
                    cmd.Parameters.AddWithValue("@Bagnum", Convert.ToInt32(model.BagNumber));

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        if (ds.Tables[0].Rows[0][0].ToString() == "0")
                            return (true, "Float return required.");
                        else
                            return (true, "Cashup ready.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return (false, "Please check your seal number ! Please enter correct seal number ! \r\n An error occurred: " + ex.Message);
        }

        
    }

    public int SaveCashupDeclaration(string userName, int bagNumber, string shift, DateTime operationDay, CashDenomination cashDenomination, int NEFT_CHEQUE_CARD_Amount)
    {
        int result = 0;
        //return 1;

        bool isInserted = InsertCashData(userName, bagNumber, shift, operationDay, cashDenomination, NEFT_CHEQUE_CARD_Amount);

        if(isInserted) {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("[TMS_SM].[dbo].[sp_SaveCashupDeclaration]", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@UserName", userName);
                        cmd.Parameters.AddWithValue("@BagNumber", bagNumber);
                        cmd.Parameters.AddWithValue("@Shift", shift);
                        cmd.Parameters.AddWithValue("@OperationDay", operationDay);

                        SqlParameter returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(returnParam);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        result = Convert.ToInt32(returnParam.Value);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }               

        return result;
    }

    private bool InsertCashData(string userName, int bagNumber, string shift, DateTime operationDay, CashDenomination cashDenomination, int neftChequeCardAmount)
    {
        try
        {
         
            if (neftChequeCardAmount == 0)
            {
                // Check WIM validation
                bool isWimValid = CheckWimValidation(userName, bagNumber);
                if(!isWimValid)
                {
                    // Validation failed for WIM cashup without check amount
                    return false;
                }
            }

            // Insert cash-up data
            bool isCashupDataInserted = InsertCashupData(userName, bagNumber, shift, operationDay, cashDenomination);
            if (!isCashupDataInserted)
            {
                //LogException.ExceptionLog("InsertCashData", "Failed to insert cashup data", $"User: {userName}, Bag: {bagNumber}");
                return false;
            }

            // Log event for tracking
            string logMessage = $"Your cashup has been done succeffuly for the user:- {userName}  for the bagnumber :- {bagNumber} | " +
                                $"Denominations => 2000: {cashDenomination.INR2000}, "+
                                $"500: {cashDenomination.INR500}, 200: {cashDenomination.INR200}, 100: {cashDenomination.INR100}, " +
                                $"50: {cashDenomination.INR50}, 20: {cashDenomination.INR20}, 10: {cashDenomination.INR10}, " +
                                $"5: {cashDenomination.Coin5}, 2: {cashDenomination.Coin2}, 1: {cashDenomination.Coin1}, " +
                                $"NEFT/CHEQUE/CARD Amount: {neftChequeCardAmount}";

            bool isInsertEvent = InsertEvent(logMessage, userName);
            if (!isInsertEvent)
            {
                return false;
                //LogException.ExceptionLog("InsertCashData", "Failed to log cashup event", logMessage);
                // Not critical, so we continue
            }
            return true; // Success
        }
        catch (Exception ex)
        {
            //LogException.ExceptionLog(ex.Source, ex.Message, ex.StackTrace);
            return false; // Failure
        }
    }

    //Check WIM Validation
    private bool CheckWimValidation(string userId, int bagNumber)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[Cashup_sp].[CashupWim_CheckUser]", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Bagnumber", bagNumber);

                    conn.Open();

                    // Use ExecuteScalar to get the first column of the first row
                    object resultObj = cmd.ExecuteScalar();
                    int count = Convert.ToInt32(resultObj);

                    if (count == 0)
                        return false;
                    else
                        return true;
                }
            }

        }
        catch (Exception ex) {
            return false;
            //LogException.ExceptionLog(ex.Source, ex.Message, ex.StackTrace); 
        }
    }

    // Insert Cashup
    public bool InsertCashupData(string userName, int bagNumber, string shift, DateTime operationDay, CashDenomination cash)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[CashUp_sp].[Cashup_InsertCashupDetails]", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    cmd.Parameters.AddWithValue("@UserID", userName);
                    cmd.Parameters.AddWithValue("@BagNumber", bagNumber);
                    cmd.Parameters.AddWithValue("@ShiftType", shift);
                    cmd.Parameters.AddWithValue("@OperationDay", operationDay);
                    cmd.Parameters.AddWithValue("@Rs2000", cash.INR2000);
                    cmd.Parameters.AddWithValue("@Rs500", cash.INR500);
                    cmd.Parameters.AddWithValue("@Rs200", cash.INR200);
                    cmd.Parameters.AddWithValue("@Rs100", cash.INR100);
                    cmd.Parameters.AddWithValue("@Rs50", cash.INR50);
                    cmd.Parameters.AddWithValue("@Rs20", cash.INR20);
                    cmd.Parameters.AddWithValue("@Rs10", cash.INR10);
                    cmd.Parameters.AddWithValue("@Rs5", cash.Coin5);
                    cmd.Parameters.AddWithValue("@Rs2", cash.Coin2);
                    cmd.Parameters.AddWithValue("@Rs1", cash.Coin1);
                    cmd.Parameters.AddWithValue("@CheckAmount", 0.0); // optional
                    cmd.Parameters.AddWithValue("@WalletAmount", 0);   // optional
                    cmd.Parameters.AddWithValue("@CashupFlag", "M");

                    conn.Open();

                    // ExecuteScalar: assume stored procedure returns 1 if success
                    object resultObj = cmd.ExecuteScalar();
                    int result = Convert.ToInt32(resultObj);

                    return result > 0; // true if inserted, false otherwise
                }
            }
        }
        catch
        {
            return false;
        }
    }

    // Insert Event Log
    public bool InsertEvent(string eventlog, string user)
    {
        bool result = false;
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[TMS_SM].[Cashup_sp].[Cashup_EventLog]", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PlazaID", 1);
                    cmd.Parameters.AddWithValue("@SystemID", 4);
                    cmd.Parameters.AddWithValue("@IP", CurrentDeviceIP);
                    cmd.Parameters.AddWithValue("@UserID", user);
                    cmd.Parameters.AddWithValue("@Event", user + ":-" + eventlog);

                    conn.Open();

                    // ExecuteNonQuery for update/insert/delete
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // Optionally check if rows were affected
                    if (rowsAffected > 0)
                    {
                        result = true;
                        // Success
                    }

                }
            }

        }
        catch (Exception ex)
        {
            // Log exception
            // LogException.ExceptionLog(ex.Source, ex.Message, ex.StackTrace); 
        }
        return result;
    }


    // Get CashUpDecelration List
    public List<CashupDetails> GetCashupConsolidatedReport(string operationDay, string shift, string tcid)
    {
        var result = new List<CashupDetails>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand("[TMS_SM].[CashUp_sp].[Cashup_ConsolidatedCashupReport]", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            // Parameters
            cmd.Parameters.AddWithValue("@Operationday", operationDay);
            cmd.Parameters.AddWithValue("@Shift", shift);
            cmd.Parameters.AddWithValue("@Tcid", tcid);

            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var cashup = new CashupDetails
                    {
                        UserID = reader["UserID"]?.ToString(),
                        StartOfShiftDateTime = reader["StartOfShiftDateTime"] != DBNull.Value ? Convert.ToDateTime(reader["StartOfShiftDateTime"]) : DateTime.MinValue,
                        EndOfShiftDateTime = reader["EndOfShiftDateTime"] != DBNull.Value ? Convert.ToDateTime(reader["EndOfShiftDateTime"]) : DateTime.MinValue,
                        Bagnumber = reader["Bagnumber"] != DBNull.Value ? Convert.ToInt32(reader["Bagnumber"]) : 0,
                        Shiftnnumber = reader["Shiftnnumber"] != DBNull.Value ? Convert.ToInt32(reader["Shiftnnumber"]) : 0,

                        CoinOf5Rupees = reader["CoinOf5Rupees"] != DBNull.Value ? Convert.ToInt32(reader["CoinOf5Rupees"]) : 0,
                        CoinOf2Rupees = reader["CoinOf2Rupees"] != DBNull.Value ? Convert.ToInt32(reader["CoinOf2Rupees"]) : 0,
                        CoinOf1Rupee = reader["CoinOf1Rupee"] != DBNull.Value ? Convert.ToInt32(reader["CoinOf1Rupee"]) : 0,

                        NoteOf1Rupee = reader["NoteOf1Rupee"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf1Rupee"]) : 0,
                        NoteOf2Rupees = reader["NoteOf2Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf2Rupees"]) : 0,
                        NoteOf5Rupees = reader["NoteOf5Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf5Rupees"]) : 0,
                        NoteOf10Rupees = reader["NoteOf10Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf10Rupees"]) : 0,
                        NoteOf20Rupees = reader["NoteOf20Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf20Rupees"]) : 0,
                        NoteOf50Rupees = reader["NoteOf50Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf50Rupees"]) : 0,
                        NoteOf100Rupees = reader["NoteOf100Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf100Rupees"]) : 0,
                        NoteOf200Rupees = reader["NoteOf200Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf200Rupees"]) : 0,
                        NoteOf500Rupees = reader["NoteOf500Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf500Rupees"]) : 0,
                        NoteOf1000Rupees = reader["NoteOf1000Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf1000Rupees"]) : 0,
                        NoteOf2000Rupees = reader["NoteOf2000Rupees"] != DBNull.Value ? Convert.ToInt32(reader["NoteOf2000Rupees"]) : 0,

                        CheckDraftAmount = reader["CheckDraftAmount"] != DBNull.Value ? Convert.ToDouble(reader["CheckDraftAmount"]) : 0,
                        WalletAmount = reader["WalletAmount"] != DBNull.Value ? Convert.ToDouble(reader["WalletAmount"]) : 0,

                        TCashupAmount = reader["TCashupAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TCashupAmount"]) : 0,
                        CashupDeclareAmount = reader["CashupDeclareAmount"] != DBNull.Value ? Convert.ToDecimal(reader["CashupDeclareAmount"]) : 0,
                        FloatAmount = reader["FloatAmount"] != DBNull.Value ? Convert.ToDecimal(reader["FloatAmount"]) : 0,

                        BeforValidationtionSystemAmount = reader["BeforValidationtionSystemAmount"] != DBNull.Value ? Convert.ToDecimal(reader["BeforValidationtionSystemAmount"]) : 0,
                        Shortage = reader["Shortage"] != DBNull.Value ? Convert.ToDecimal(reader["Shortage"]) : 0,
                        Excess = reader["Excess"] != DBNull.Value ? Convert.ToDecimal(reader["Excess"]) : 0,
                        AfterValidationSystemAmount = reader["AfterValidationSystemAmount"] != DBNull.Value ? Convert.ToDecimal(reader["AfterValidationSystemAmount"]) : 0,
                        ExcessShortage = reader["ExcessShortage"] != DBNull.Value ? Convert.ToDecimal(reader["ExcessShortage"]) : 0,
                        BleedAmount = reader["BleedAmount"] != DBNull.Value ? Convert.ToDecimal(reader["BleedAmount"]) : 0,
                        TSystemWIMamount = reader["TSystemWIMamount"] != DBNull.Value ? Convert.ToDecimal(reader["TSystemWIMamount"]) : 0,
                        Paytm = reader["Paytm"] != DBNull.Value ? Convert.ToDecimal(reader["Paytm"]) : 0,
                        Wimonle = reader["Wimonle"] != DBNull.Value ? Convert.ToDecimal(reader["Wimonle"]) : 0,

                        ShiftTypeID = reader["ShiftTypeID"] != DBNull.Value ? Convert.ToInt32(reader["ShiftTypeID"]) : 0
                    };

                    result.Add(cashup);
                }
            }
        }

        return result;
    }

    public List<JobPosition> GetJobPositions()
    {
        return new List<JobPosition>
        {
            new JobPosition { Id = 1, Name = "Administrator" },
            new JobPosition { Id = 2, Name = "Manager" },
            new JobPosition { Id = 3, Name = "Supervisor" },
            new JobPosition { Id = 4, Name = "Operator" },
            new JobPosition { Id = 5, Name = "Viewer" }
        };
    }

    public List<UserRight> GetAllRights()
    {
        return new List<UserRight>
        {
            new UserRight { Id = 1, RightName = "Dashboard View" },
            new UserRight { Id = 2, RightName = "User Management" },
            new UserRight { Id = 3, RightName = "Add Employee" },
            new UserRight { Id = 4, RightName = "Edit Employee" },
            new UserRight { Id = 5, RightName = "View Reports" },
            new UserRight { Id = 6, RightName = "Permission Setup" },
            new UserRight { Id = 7, RightName = "Delete Records" },
            new UserRight { Id = 8, RightName = "Manage Salary" },
            new UserRight { Id = 9, RightName = "Leave Approval" },
            new UserRight { Id = 10, RightName = "Shift Assignment" }
        };
    }

}
