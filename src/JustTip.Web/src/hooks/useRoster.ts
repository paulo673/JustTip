import { useQuery } from '@tanstack/react-query'
import { format, startOfWeek, addDays } from 'date-fns'
import { api } from '../lib/api'
import type { ShiftDto, EmployeeRosterDto, RosterData } from '../types/roster'

async function fetchRoster(weekStart: Date): Promise<RosterData> {
  const formattedDate = format(weekStart, 'yyyy-MM-dd')
  const response = await api.get<ShiftDto[]>(`/roster?startDate=${formattedDate}`)
  const shifts = response.data

  const employeeMap = new Map<number, EmployeeRosterDto>()

  shifts.forEach((shift) => {
    if (!employeeMap.has(shift.employeeId)) {
      employeeMap.set(shift.employeeId, {
        employeeId: shift.employeeId,
        name: shift.employeeName,
        shifts: [],
      })
    }
    employeeMap.get(shift.employeeId)!.shifts.push(shift)
  })

  return {
    employees: Array.from(employeeMap.values()),
    weekStart,
    weekEnd: addDays(weekStart, 6),
  }
}

export function useRoster(selectedDate: Date) {
  const weekStart = startOfWeek(selectedDate, { weekStartsOn: 1 })

  return useQuery({
    queryKey: ['roster', format(weekStart, 'yyyy-MM-dd')],
    queryFn: () => fetchRoster(weekStart),
  })
}

export function getWeekDays(weekStart: Date) {
  return Array.from({ length: 7 }, (_, i) => addDays(weekStart, i))
}
