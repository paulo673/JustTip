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
  totalHours: number
}

export interface WeeklyRosterDto {
  employees: EmployeeRosterDto[]
}

export interface RosterData {
  employees: EmployeeRosterDto[]
  weekStart: Date
  weekEnd: Date
}
