export interface ShiftDto {
  id: number
  employeeId: number
  employeeName: string
  date: string
  startTime: string
  endTime: string
}

export interface EmployeeRosterDto {
  employeeId: number
  name: string
  shifts: ShiftDto[]
}

export interface RosterData {
  employees: EmployeeRosterDto[]
  weekStart: Date
  weekEnd: Date
}
