﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotorRental.Entities;
using MotorRental.Infrastructure.Presentation.Extension;
using MotorRental.Infrastructure.Presentation.Models;
using MotorRental.Infrastructure.Presentation.Models.DTO;
using MotorRental.MotorRental.UseCase;
using MotorRental.UseCase;
using MotorRental.Utilities;
using System.Net;

namespace MotorRental.Infrastructure.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IMapper _mapper;
        private ApiResponse _response;

        public AppointmentController(IAppointmentService appointmentService,
                                     IMapper mapper)
        {
            _appointmentService = appointmentService;
            _mapper = mapper;
            _response = new ApiResponse();
        }

        [HttpPost]
        [Authorize(Roles = "Visitor")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> CreateAppoinment([FromForm] CreateAppoinmentDTO request)
        {
            // comvert DTO to domain
            var domain = _mapper.Map<Appointment>(request);
            domain.CustomerId = HttpContext.GetUserId();
            
            // call service
            var res = await _appointmentService.CreateAppoitment(domain);

            if (res.isSucess == false)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { res.ErrorMessage };
                return BadRequest(_response);
            }
            else
            {
                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                return Ok(_response);
            }
        }

        [HttpGet("GetAppointment")]
        [Authorize(Roles = "Owner,Visitor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> GetAllAppointments([FromQuery] AppointmentFindCreterias creterias,
                                                                        [FromQuery] AppointmentSortBy sortBy = AppointmentSortBy.DateAscending)
        {
            var userId = HttpContext.GetUserId();
            var role = HttpContext.GetRole();

            var res = await _appointmentService.GetAllApointment(userId, role, creterias, sortBy);
            if (res == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { "Please try again" };
                return BadRequest(_response);
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = res;
                return Ok(_response);
            }
        }
    }
}