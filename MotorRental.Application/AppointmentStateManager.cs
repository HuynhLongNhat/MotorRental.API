﻿using MotorRental.Entities;
using MotorRental.UseCase.UnitOfWork;
using MotorRental.Utilities;
using MotorRental.UseCase.Helper;

namespace MotorRental.UseCase
{
    public class AppointmentStateManager : IAppointmentStateManager
    {
        private readonly IAppointmentUnitOfWork _appointmentUnitOfWork;

        public AppointmentStateManager(IAppointmentUnitOfWork appointmentUnitOfWork)
        {
            _appointmentUnitOfWork = appointmentUnitOfWork;
        }

        public async Task<TransactionResult> CreateAppoitment(Appointment appointment)
        {
            try
            {
                // begin transaction
                await _appointmentUnitOfWork.BeginTransaction();

                //check information user is valid
                var existingUser = await _appointmentUnitOfWork.UserRepository.GetById(appointment.CustomerId);
                if (!ValidationOptionMotorbike.CheckIformationInvalid(existingUser))
                {
                    return TransactionResult.InforUserInvalid;
                }

                // check motorbike is free
                var existingMobike = await _appointmentUnitOfWork
                    .MotorRepository
                    .GetByIdAndUserId(appointment.MotorbikeId, appointment.OwnerId);

                if (!ValidationOptionMotorbike.CheckMotorbikeFree(existingMobike))
                {
                    return TransactionResult.MotorbikeCanNotUseNow;
                }

                // create appoinment
                appointment.StatusAppointment = SD.Status_Payment_Not;
                appointment.StatusAppointment = SD.Status_Appointment_Process;
                appointment.CreatedAt = DateTime.Now;
                var res = await _appointmentUnitOfWork.AppointmentRepository
                                                .CreateAppoinment(appointment);

                // update status motorbike
                existingMobike.status = SD.Status_Busy;
                var motorUpdate = _appointmentUnitOfWork.MotorRepository
                                                .UpdateNotSave(existingMobike);

                if (motorUpdate.status != SD.Status_Busy)
                {
                    await _appointmentUnitOfWork.Cancel();
                    throw new Exception(message: "Error happening, Please try again");
                }
                else
                {
                    await _appointmentUnitOfWork.SaveChanges();
                }

                return TransactionResult.Success;
            }
            catch (Exception ex)
            {
                return new TransactionResult() { isSucess = false, ErrorMessage = ex.Message };
            }
        }

        
    }
}