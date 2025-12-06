import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { format, parseISO, isBefore, startOfDay } from 'date-fns'
import { CalendarIcon, Loader2, Trash2 } from 'lucide-react'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '../ui/dialog'
import { Button } from '../ui/button'
import { Input } from '../ui/input'
import { Label } from '../ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../ui/select'
import { Popover, PopoverContent, PopoverTrigger } from '../ui/popover'
import { Calendar } from '../ui/calendar'
import { cn } from '../../lib/utils'
import { useEmployees } from '../../hooks/useEmployees'
import { useCreateShift } from '../../hooks/useCreateShift'
import { useUpdateShift } from '../../hooks/useUpdateShift'
import { useDeleteShift } from '../../hooks/useDeleteShift'
import type { ShiftDto } from '../../types/roster'

const shiftSchema = z
  .object({
    employeeId: z.string().min(1, 'Employee is required'),
    date: z.date({ message: 'Date is required' }),
    startTime: z.string().min(1, 'Start time is required'),
    endTime: z.string().min(1, 'End time is required'),
  })
  .refine(
    (data) => {
      if (!data.startTime || !data.endTime) return true
      return data.endTime > data.startTime
    },
    {
      message: 'End time must be after start time',
      path: ['endTime'],
    }
  )
  .refine(
    (data) => {
      if (!data.date) return true
      const today = startOfDay(new Date())
      return !isBefore(startOfDay(data.date), today)
    },
    {
      message: 'Cannot schedule shifts for past dates',
      path: ['date'],
    }
  )

type ShiftFormData = z.infer<typeof shiftSchema>

interface ShiftDialogProps {
  isOpen: boolean
  onClose: () => void
  shift?: ShiftDto | null
  defaultDate?: Date
}

export function ShiftDialog({
  isOpen,
  onClose,
  shift,
  defaultDate,
}: ShiftDialogProps) {
  const isEditing = !!shift
  const { data: employees, isLoading: isLoadingEmployees } = useEmployees()
  const createMutation = useCreateShift()
  const updateMutation = useUpdateShift()
  const deleteMutation = useDeleteShift()

  const isPastShift = shift
    ? isBefore(startOfDay(parseISO(shift.date)), startOfDay(new Date()))
    : false

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors },
  } = useForm<ShiftFormData>({
    resolver: zodResolver(shiftSchema),
    defaultValues: {
      employeeId: '',
      date: defaultDate || new Date(),
      startTime: '09:00',
      endTime: '17:00',
    },
  })

  const selectedDate = watch('date')
  const selectedEmployeeId = watch('employeeId')

  useEffect(() => {
    if (isOpen) {
      if (shift) {
        reset({
          employeeId: shift.employeeId.toString(),
          date: parseISO(shift.date),
          startTime: shift.startTime.substring(0, 5),
          endTime: shift.endTime.substring(0, 5),
        })
      } else {
        reset({
          employeeId: '',
          date: defaultDate || new Date(),
          startTime: '09:00',
          endTime: '17:00',
        })
      }
    }
  }, [isOpen, shift, defaultDate, reset])

  const onSubmit = async (data: ShiftFormData) => {
    const payload = {
      employeeId: parseInt(data.employeeId, 10),
      date: format(data.date, 'yyyy-MM-dd'),
      startTime: data.startTime,
      endTime: data.endTime,
    }

    try {
      if (isEditing && shift) {
        await updateMutation.mutateAsync({ id: shift.id, request: payload })
      } else {
        await createMutation.mutateAsync(payload)
      }
      onClose()
    } catch {
      // Error is handled by mutation
    }
  }

  const handleDelete = async () => {
    if (!shift) return

    try {
      await deleteMutation.mutateAsync(shift.id)
      onClose()
    } catch {
      // Error is handled by mutation
    }
  }

  const isSubmitting =
    createMutation.isPending ||
    updateMutation.isPending ||
    deleteMutation.isPending

  const apiError =
    createMutation.error || updateMutation.error || deleteMutation.error

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? 'Edit Shift' : 'Add New Shift'}
          </DialogTitle>
          <DialogDescription>
            {isEditing
              ? 'Update the shift details below.'
              : 'Schedule a shift for an employee.'}
          </DialogDescription>
        </DialogHeader>

        {isPastShift && (
          <div className="rounded-md bg-amber-50 border border-amber-200 p-3">
            <p className="text-sm text-amber-700">
              This shift is in the past and cannot be modified.
            </p>
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="employee">Employee</Label>
            <Select
              value={selectedEmployeeId}
              onValueChange={(value) => setValue('employeeId', value)}
              disabled={isLoadingEmployees || isPastShift}
            >
              <SelectTrigger id="employee">
                <SelectValue placeholder="Select an employee" />
              </SelectTrigger>
              <SelectContent>
                {employees?.map((employee) => (
                  <SelectItem
                    key={employee.id}
                    value={employee.id.toString()}
                  >
                    {employee.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.employeeId && (
              <p className="text-sm text-red-500">{errors.employeeId.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label>Date</Label>
            <Popover>
              <PopoverTrigger asChild>
                <Button
                  variant="outline"
                  disabled={isPastShift}
                  className={cn(
                    'w-full justify-start text-left font-normal',
                    !selectedDate && 'text-slate-500'
                  )}
                >
                  <CalendarIcon className="mr-2 h-4 w-4" />
                  {selectedDate ? (
                    format(selectedDate, 'PPP')
                  ) : (
                    <span>Pick a date</span>
                  )}
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-auto p-0" align="start">
                <Calendar
                  mode="single"
                  selected={selectedDate}
                  onSelect={(date) => date && setValue('date', date)}
                  disabled={(date) =>
                    isBefore(startOfDay(date), startOfDay(new Date()))
                  }
                  initialFocus
                />
              </PopoverContent>
            </Popover>
            {errors.date && (
              <p className="text-sm text-red-500">{errors.date.message}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="startTime">Start Time</Label>
              <Input
                id="startTime"
                type="time"
                disabled={isPastShift}
                {...register('startTime')}
              />
              {errors.startTime && (
                <p className="text-sm text-red-500">
                  {errors.startTime.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="endTime">End Time</Label>
              <Input
                id="endTime"
                type="time"
                disabled={isPastShift}
                {...register('endTime')}
              />
              {errors.endTime && (
                <p className="text-sm text-red-500">{errors.endTime.message}</p>
              )}
            </div>
          </div>

          {apiError && (
            <div className="rounded-md bg-red-50 border border-red-200 p-3">
              <p className="text-sm text-red-600">
                {(apiError as Error & { response?: { data?: { error?: string } } })
                  ?.response?.data?.error || 'An error occurred'}
              </p>
            </div>
          )}

          <DialogFooter className="gap-2 sm:gap-0">
            {isEditing && !isPastShift && (
              <Button
                type="button"
                variant="destructive"
                onClick={handleDelete}
                disabled={isSubmitting}
                className="mr-auto"
              >
                {deleteMutation.isPending ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Trash2 className="h-4 w-4" />
                )}
                <span className="ml-2">Delete</span>
              </Button>
            )}
            <Button
              type="button"
              variant="ghost"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            {!isPastShift && (
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting && (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                {isEditing ? 'Save Changes' : 'Add Shift'}
              </Button>
            )}
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
