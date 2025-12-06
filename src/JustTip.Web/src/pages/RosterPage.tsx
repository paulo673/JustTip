import { useState } from 'react'
import { Plus } from 'lucide-react'
import { startOfWeek } from 'date-fns'
import { Navbar } from '../components/layout/Navbar'
import { WeekNavigator } from '../components/roster/WeekNavigator'
import { RosterGrid } from '../components/roster/RosterGrid'
import { ShiftDialog } from '../components/roster/ShiftDialog'
import { useRoster } from '../hooks/useRoster'
import type { ShiftDto } from '../types/roster'

export function RosterPage() {
  const [selectedDate, setSelectedDate] = useState(() => new Date())
  const [isDialogOpen, setIsDialogOpen] = useState(false)
  const [selectedShift, setSelectedShift] = useState<ShiftDto | null>(null)
  const weekStart = startOfWeek(selectedDate, { weekStartsOn: 1 })

  const { data, isLoading, isError, error } = useRoster(selectedDate)

  const handleAddShift = () => {
    setSelectedShift(null)
    setIsDialogOpen(true)
  }

  const handleShiftClick = (shift: ShiftDto) => {
    setSelectedShift(shift)
    setIsDialogOpen(true)
  }

  const handleCloseDialog = () => {
    setIsDialogOpen(false)
    setSelectedShift(null)
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <Navbar activePage="roster" />

      <main className="max-w-7xl mx-auto px-6 py-8">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-2xl font-semibold text-slate-900">Staff Roster</h1>

          <WeekNavigator
            selectedDate={selectedDate}
            onDateChange={setSelectedDate}
          />

          <button
            onClick={handleAddShift}
            className="flex items-center gap-2 px-5 py-2.5 rounded-lg bg-slate-900 text-white font-medium text-sm hover:bg-slate-800 transition-colors shadow-sm"
          >
            <Plus className="h-4 w-4" strokeWidth={2.5} />
            Add Shift
          </button>
        </div>

        {isLoading && (
          <div className="bg-white rounded-xl border border-slate-200 p-12 shadow-sm">
            <div className="flex items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-2 border-slate-200 border-t-slate-900" />
              <span className="ml-4 text-slate-500 font-medium">Loading roster...</span>
            </div>
          </div>
        )}

        {isError && (
          <div className="bg-red-50 border border-red-200 rounded-xl p-6">
            <p className="text-red-600 font-medium">
              Error loading roster: {error?.message || 'Unknown error'}
            </p>
          </div>
        )}

        {data && (
          <RosterGrid
            employees={data.employees}
            weekStart={weekStart}
            onShiftClick={handleShiftClick}
          />
        )}
      </main>

      <ShiftDialog
        isOpen={isDialogOpen}
        onClose={handleCloseDialog}
        shift={selectedShift}
        defaultDate={selectedDate}
      />
    </div>
  )
}
