import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { format, addDays, startOfWeek } from 'date-fns'
import { Calculator, Loader2 } from 'lucide-react'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import type { DailyTipInput, DailyTipResponse } from '../../hooks/useTips'

interface DailyInputGridProps {
  selectedDate: Date
  existingTips?: DailyTipResponse[]
  onSubmit: (tips: DailyTipInput[]) => void
  isSubmitting: boolean
}

interface FormValues {
  tips: { [key: string]: string }
}

const DAY_LABELS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

export function DailyInputGrid({ selectedDate, existingTips, onSubmit, isSubmitting }: DailyInputGridProps) {
  const weekStart = startOfWeek(selectedDate, { weekStartsOn: 1 })
  const weekDays = Array.from({ length: 7 }, (_, i) => addDays(weekStart, i))

  const buildDefaultValues = () => {
    const tipsByDate = new Map(existingTips?.map(t => [t.date, t.amount]) ?? [])
    return weekDays.reduce((acc, day) => {
      const dateKey = format(day, 'yyyy-MM-dd')
      const existingAmount = tipsByDate.get(dateKey)
      acc[dateKey] = existingAmount ? existingAmount.toString() : ''
      return acc
    }, {} as { [key: string]: string })
  }

  const { register, handleSubmit, watch, reset } = useForm<FormValues>({
    defaultValues: {
      tips: buildDefaultValues()
    }
  })

  useEffect(() => {
    reset({ tips: buildDefaultValues() })
  }, [existingTips])

  const tipValues = watch('tips') || {}
  const totalTips = Object.values(tipValues).reduce((sum, val) => {
    const num = parseFloat(val as string) || 0
    return sum + num
  }, 0)

  const processSubmit = (data: FormValues) => {
    const tips: DailyTipInput[] = weekDays
      .map(day => {
        const dateKey = format(day, 'yyyy-MM-dd')
        const amount = parseFloat(data.tips[dateKey]) || 0
        return { date: dateKey, amount }
      })
      .filter(tip => tip.amount > 0)

    onSubmit(tips)
  }

  return (
    <form onSubmit={handleSubmit(processSubmit)} className="space-y-6">
      <div className="grid grid-cols-7 gap-3">
        {weekDays.map((day, index) => {
          const dateKey = format(day, 'yyyy-MM-dd')
          return (
            <div
              key={dateKey}
              className="bg-white rounded-xl border border-slate-200 p-4 shadow-sm"
            >
              <div className="text-center mb-3">
                <p className="text-sm font-semibold text-slate-900">{DAY_LABELS[index]}</p>
                <p className="text-xs text-slate-500">{format(day, 'MMM d')}</p>
              </div>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm">
                  €
                </span>
                <Input
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="0.00"
                  className="pl-7 text-right"
                  {...register(`tips.${dateKey}`)}
                />
              </div>
            </div>
          )
        })}
      </div>

      <div className="flex items-center justify-between">
        <div className="text-sm text-slate-600">
          <span className="font-medium">Preview Total:</span>{' '}
          <span className="text-lg font-semibold text-slate-900">€{totalTips.toFixed(2)}</span>
        </div>

        <Button
          type="submit"
          disabled={isSubmitting || totalTips === 0}
          className="flex items-center gap-2"
        >
          {isSubmitting ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <Calculator className="h-4 w-4" />
          )}
          Calculate & Save
        </Button>
      </div>
    </form>
  )
}
