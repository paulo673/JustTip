import { ChevronLeft, ChevronRight, Calendar } from 'lucide-react'
import { format, addWeeks, subWeeks, startOfWeek, addDays } from 'date-fns'

interface WeekNavigatorProps {
  selectedDate: Date
  onDateChange: (date: Date) => void
}

export function WeekNavigator({ selectedDate, onDateChange }: WeekNavigatorProps) {
  const weekStart = startOfWeek(selectedDate, { weekStartsOn: 1 })
  const weekEnd = addDays(weekStart, 6)

  const handlePrevious = () => {
    onDateChange(subWeeks(selectedDate, 1))
  }

  const handleNext = () => {
    onDateChange(addWeeks(selectedDate, 1))
  }

  return (
    <div className="flex items-center gap-1">
      <button
        onClick={handlePrevious}
        className="p-2 rounded-lg text-slate-500 hover:text-slate-700 hover:bg-slate-100 transition-colors"
        aria-label="Previous week"
      >
        <ChevronLeft className="h-5 w-5" />
      </button>

      <div className="flex items-center gap-2.5 px-4 py-2 bg-white border border-slate-200 rounded-lg min-w-[280px] justify-center shadow-sm">
        <Calendar className="h-4 w-4 text-slate-400" />
        <span className="text-sm font-medium text-slate-700">
          Week of {format(weekStart, 'MMM dd')} - {format(weekEnd, 'MMM dd, yyyy')}
        </span>
      </div>

      <button
        onClick={handleNext}
        className="p-2 rounded-lg text-slate-500 hover:text-slate-700 hover:bg-slate-100 transition-colors"
        aria-label="Next week"
      >
        <ChevronRight className="h-5 w-5" />
      </button>
    </div>
  )
}
