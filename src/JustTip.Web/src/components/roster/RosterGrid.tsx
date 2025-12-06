import { format, isSameDay, parseISO, isToday } from 'date-fns'
import { cn } from '../../lib/utils'
import { getWeekDays } from '../../hooks/useRoster'
import type { EmployeeRosterDto, ShiftDto } from '../../types/roster'

interface RosterGridProps {
  employees: EmployeeRosterDto[]
  weekStart: Date
}

const avatarColors = [
  'bg-blue-100 text-blue-700',
  'bg-emerald-100 text-emerald-700',
  'bg-amber-100 text-amber-700',
  'bg-purple-100 text-purple-700',
  'bg-rose-100 text-rose-700',
  'bg-cyan-100 text-cyan-700',
]

function Avatar({ name, index }: { name: string; index: number }) {
  const initials = name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)

  const colorClass = avatarColors[index % avatarColors.length]

  return (
    <div
      className={cn(
        "w-9 h-9 rounded-full flex items-center justify-center text-xs font-semibold",
        colorClass
      )}
    >
      {initials}
    </div>
  )
}

function ShiftPill({ shift }: { shift: ShiftDto }) {
  const startHour = parseInt(shift.startTime.split(':')[0], 10)
  const isEvening = startHour >= 17

  const formatTime = (time: string) => {
    const [hours, minutes] = time.split(':')
    return `${hours}:${minutes}`
  }

  return (
    <div
      className={cn(
        "px-3 py-1.5 rounded-md text-xs font-medium whitespace-nowrap",
        isEvening
          ? "bg-slate-100 text-slate-700"
          : "bg-blue-50 text-blue-700"
      )}
    >
      {formatTime(shift.startTime)} - {formatTime(shift.endTime)}
    </div>
  )
}

function DayCell({ shifts, date }: { shifts: ShiftDto[]; date: Date }) {
  const dayShifts = shifts.filter((shift) => {
    const shiftDate = parseISO(shift.date)
    return isSameDay(shiftDate, date)
  })

  if (dayShifts.length === 0) {
    return (
      <span className="text-slate-300 text-sm font-medium">Off</span>
    )
  }

  return (
    <div className="flex flex-col gap-1.5">
      {dayShifts.map((shift) => (
        <ShiftPill key={shift.id} shift={shift} />
      ))}
    </div>
  )
}

export function RosterGrid({ employees, weekStart }: RosterGridProps) {
  const weekDays = getWeekDays(weekStart)

  return (
    <div className="bg-white rounded-xl border border-slate-200 overflow-hidden shadow-sm">
      <table className="w-full border-collapse">
        <thead>
          <tr className="bg-slate-50 border-b border-slate-200">
            <th className="text-left py-4 px-5 text-sm font-semibold text-slate-600 w-52">
              Employee
            </th>
            {weekDays.map((day) => (
              <th
                key={day.toISOString()}
                className={cn(
                  "py-4 px-3 text-center text-sm font-semibold",
                  isToday(day)
                    ? "text-blue-600 bg-blue-50/50"
                    : "text-slate-600"
                )}
              >
                {format(day, 'EEE dd')}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {employees.length === 0 ? (
            <tr>
              <td colSpan={8} className="py-16 text-center">
                <p className="text-slate-400 font-medium">No employees or shifts found for this week.</p>
              </td>
            </tr>
          ) : (
            employees.map((employee, index) => (
              <tr
                key={employee.employeeId}
                className="hover:bg-slate-50/50 transition-colors"
              >
                <td className="py-4 px-5">
                  <div className="flex items-center gap-3">
                    <Avatar name={employee.name} index={index} />
                    <span className="text-sm font-medium text-slate-800">
                      {employee.name}
                    </span>
                  </div>
                </td>
                {weekDays.map((day) => (
                  <td
                    key={day.toISOString()}
                    className={cn(
                      "py-4 px-3 text-center align-middle",
                      isToday(day) && "bg-blue-50/30"
                    )}
                  >
                    <div className="flex items-center justify-center min-h-[36px]">
                      <DayCell shifts={employee.shifts} date={day} />
                    </div>
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )
}
